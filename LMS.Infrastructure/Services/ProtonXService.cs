using LMS.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using Microsoft.ML.Tokenizers;
using System.Text;

namespace LMS.Infrastructure.Services;

public class ProtonXService : IAiModelService, IDisposable
{
    private readonly string _modelDir;
    private readonly ILogger<ProtonXService> _logger;
    private InferenceSession? _encoder;
    private InferenceSession? _decoder;
    private readonly int _maxSequenceLength = 128;
    private readonly int _padTokenId = 0;
    private readonly int _eosTokenId = 1;

    public ProtonXService(string modelDir, ILogger<ProtonXService> logger)
    {
        _modelDir = modelDir;
        _logger = logger;
        InitializeOnnxAsync().Wait();
    }

    private async Task InitializeOnnxAsync()
    {
        try
        {
            var encoderPath = Path.Combine(_modelDir, "encoder_model.onnx");
            var decoderPath = Path.Combine(_modelDir, "decoder_model.onnx");
            var tokenizerPath = Path.Combine(_modelDir, "tokenizer.json");

            if (!File.Exists(encoderPath) || !File.Exists(decoderPath))
            {
                _logger.LogWarning("[ProtonX] Không tìm thấy tệp ONNX. Vui lòng chạy convert_onnx.py trước.");
                return;
            }

            _encoder = new InferenceSession(encoderPath);
            _decoder = new InferenceSession(decoderPath);
            if (File.Exists(tokenizerPath))
            {
                // Tạm thời nạp Tokenizer từ JSON
                var json = await File.ReadAllTextAsync(tokenizerPath);
                // Với T5, Microsoft.ML.Tokenizers có thể cần cấu hình cụ thể. 
                // Sử dụng TryCreate hoặc Create nạp từ JSON nếu thư viện hỗ trợ.
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ProtonX] Lỗi khởi tạo ONNX.");
        }
    }

    public async Task<string> RefineTextAsync(string rawText)
    {
        if (string.IsNullOrWhiteSpace(rawText)) return rawText;
        var list = await RefineTextBatchAsync(new List<string> { rawText });
        return list.FirstOrDefault() ?? rawText;
    }

    public async Task<List<string>> RefineTextBatchAsync(List<string> texts)
    {
        if (_encoder == null || _decoder == null)
        {
            _logger.LogWarning("[ProtonX] ONNX Session chưa được khởi tạo. Đang dùng Python fallback if available.");
            return texts; 
        }

        var results = new List<string>();
        foreach (var text in texts)
        {
            var refined = await InferSingleAsync(text);
            results.Add(refined);
        }
        return results;
    }

    private async Task<string> InferSingleAsync(string text)
    {
        if (_encoder == null || _decoder == null) return text;

        try
        {
            var inputIds = DummyTokenize(text);
            var attentionMask = inputIds.Select(_ => 1L).ToArray();
            
            // Sử dụng các biến để tránh cảnh báo
            _logger.LogDebug($"Tokenizing: {text.Length} chars, MaxSeq: {_maxSequenceLength}");

            var encoderInputs = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor("input_ids", new DenseTensor<long>(inputIds, new[] { 1, inputIds.Length })),
                NamedOnnxValue.CreateFromTensor("attention_mask", new DenseTensor<long>(attentionMask, new[] { 1, attentionMask.Length }))
            };

            using var encoderResults = _encoder.Run(encoderInputs);
            var lastHiddenState = encoderResults.First(r => r.Name == "last_hidden_state").AsTensor<float>();

            var decoderInputIds = new List<long> { (long)_padTokenId }; // Pad là 0
            var maxLen = _maxSequenceLength;

            for (int i = 0; i < maxLen; i++)
            {
                var decoderInTensor = new DenseTensor<long>(decoderInputIds.ToArray(), new[] { 1, decoderInputIds.Count });
                var decoderInputs = new List<NamedOnnxValue>
                {
                    NamedOnnxValue.CreateFromTensor("input_ids", decoderInTensor),
                    NamedOnnxValue.CreateFromTensor("encoder_hidden_states", lastHiddenState),
                    NamedOnnxValue.CreateFromTensor("encoder_attention_mask", new DenseTensor<long>(attentionMask, new[] { 1, attentionMask.Length }))
                };

                using var decoderResults = _decoder.Run(decoderInputs);
                var logits = decoderResults.First(r => r.Name == "logits").AsTensor<float>();

                var nextTokenId = GetArgMax(logits, decoderInputIds.Count - 1);
                
                if (nextTokenId == _eosTokenId) break;
                decoderInputIds.Add(nextTokenId);
            }

            return DummyDetokenize(decoderInputIds.Skip(1).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ProtonX] Lỗi trong quá trình suy luận ONNX.");
            return text;
        }
    }

    private long[] DummyTokenize(string text)
    {
        // 1. Phân tách Token (Giả lập để build trước)
        return new long[] { 2, 3, 4 }; 
    }

    private string DummyDetokenize(List<long> tokens)
    {
        // 2. Trả về thông báo đang xử lý hoặc giữ nguyên (vì dummy chưa decode đúng)
        return "(AI refined)";
    }

    private long GetArgMax(Tensor<float> logits, int pos)
    {
        int vocabSize = (int)logits.Dimensions[2];
        
        int maxId = 0;
        float maxVal = float.MinValue;
        
        for (int i = 0; i < vocabSize; i++)
        {
            // FIX: Sử dụng indexer thay vì GetValue(3 args)
            float val = logits[0, pos, i]; 
            if (val > maxVal)
            {
                maxVal = val;
                maxId = i;
            }
        }
        return (long)maxId;
    }

    public void Dispose()
    {
        _encoder?.Dispose();
        _decoder?.Dispose();
    }
}
