using System.Net.Http.Json;
using System.Text.Json;
using WebAdmin.Models;
using Microsoft.AspNetCore.Http;

namespace WebAdmin.Services;

public class RestaurantApiClient : IRestaurantApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<RestaurantApiClient> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private const string ApiBaseUrl = "http://localhost:5137/api";
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public RestaurantApiClient(
        HttpClient httpClient, 
        ILogger<RestaurantApiClient> logger,
        IHttpContextAccessor httpContextAccessor)  // ✅ Thêm IHttpContextAccessor
    {
        _httpClient = httpClient;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    // ✅ Helper method để thêm token vào request
    private void AddAuthorizationHeader()
    {
        var token = _httpContextAccessor.HttpContext?.Request.Cookies["token"];
        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Remove("Authorization");
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
            _logger.LogInformation("🔑 Token added to request");
        }
        else
        {
            _logger.LogWarning("⚠️ No token found in cookies");
        }
    }

    // ============================================
    // DANH MUC
    // ============================================
    
    public async Task<List<DanhMucViewModel>> GetAllDanhMuc()
    {
        try
        {
            AddAuthorizationHeader();  // ✅ Thêm token
            _logger.LogInformation("🔍 GET {Url}", $"{ApiBaseUrl}/Danhmuc");
            var result = await _httpClient.GetFromJsonAsync<List<DanhMucViewModel>>($"{ApiBaseUrl}/Danhmuc", _jsonOptions);
            return result ?? new List<DanhMucViewModel>();
        }
        catch (Exception ex)
        {
            _logger.LogError("❌ Error GetAllDanhMuc: {Message}", ex.Message);
            return new List<DanhMucViewModel>();
        }
    }

    public async Task<DanhMucViewModel> GetDanhMuc(int id)
    {
        try
        {
            AddAuthorizationHeader();  // ✅ Thêm token
            _logger.LogInformation("🔍 GET {Url}", $"{ApiBaseUrl}/Danhmuc/{id}");
            return await _httpClient.GetFromJsonAsync<DanhMucViewModel>($"{ApiBaseUrl}/Danhmuc/{id}", _jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError("❌ Error GetDanhMuc: {Message}", ex.Message);
            return null;
        }
    }

    public async Task<int> CreateDanhMuc(DanhMucViewModel model)
    {
        try
        {
            AddAuthorizationHeader();  // ✅ Thêm token
            var json = JsonSerializer.Serialize(model);
            _logger.LogInformation("🔍 POST {Url}", $"{ApiBaseUrl}/Danhmuc");
            _logger.LogInformation("📤 Data: {Data}", json);
            
            var response = await _httpClient.PostAsJsonAsync($"{ApiBaseUrl}/Danhmuc", model);
            
            var responseContent = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("📡 Status: {Status}", response.StatusCode);
            _logger.LogInformation("📦 Response: {Response}", responseContent);
            
            response.EnsureSuccessStatusCode();
            
            try
            {
                return JsonSerializer.Deserialize<int>(responseContent, _jsonOptions);
            }
            catch
            {
                return 0;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("❌ Error CreateDanhMuc: {Message}", ex.Message);
            return 0;
        }
    }

    public async Task UpdateDanhMuc(int id, DanhMucViewModel model)
    {
        try
        {
            AddAuthorizationHeader();  // ✅ Thêm token
            var json = JsonSerializer.Serialize(model);
            _logger.LogInformation("🔍 PUT {Url}", $"{ApiBaseUrl}/Danhmuc/{id}");
            _logger.LogInformation("📤 Data: {Data}", json);
            
            var response = await _httpClient.PutAsJsonAsync($"{ApiBaseUrl}/Danhmuc/{id}", model);
            
            var responseContent = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("📡 Status: {Status}", response.StatusCode);
            _logger.LogInformation("📦 Response: {Response}", responseContent);
            
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            _logger.LogError("❌ Error UpdateDanhMuc: {Message}", ex.Message);
            throw;
        }
    }

    public async Task DeleteDanhMuc(int id)
    {
        try
        {
            AddAuthorizationHeader();  // ✅ Thêm token
            _logger.LogInformation("🔍 DELETE {Url}", $"{ApiBaseUrl}/Danhmuc/{id}");
            
            var response = await _httpClient.DeleteAsync($"{ApiBaseUrl}/Danhmuc/{id}");
            
            var responseContent = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("📡 Status: {Status}", response.StatusCode);
            _logger.LogInformation("📦 Response: {Response}", responseContent);
            
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            _logger.LogError("❌ Error DeleteDanhMuc: {Message}", ex.Message);
            throw;
        }
    }

    // ============================================
    // BAN AN
    // ============================================
    
    public async Task<List<BanAnViewModel>> GetAllBanAn()
    {
        try
        {
            AddAuthorizationHeader();  // ✅ Thêm token
            return await _httpClient.GetFromJsonAsync<List<BanAnViewModel>>($"{ApiBaseUrl}/Banan", _jsonOptions) ?? new List<BanAnViewModel>();
        }
        catch (Exception ex)
        {
            _logger.LogError("❌ Error GetAllBanAn: {Message}", ex.Message);
            return new List<BanAnViewModel>();
        }
    }

    public async Task<BanAnViewModel> GetBanAn(int id)
    {
        try
        {
            AddAuthorizationHeader();  // ✅ Thêm token
            return await _httpClient.GetFromJsonAsync<BanAnViewModel>($"{ApiBaseUrl}/Banan/{id}", _jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError("❌ Error GetBanAn: {Message}", ex.Message);
            return null;
        }
    }

    public async Task<int> CreateBanAn(BanAnViewModel model)
    {
        try
        {
            AddAuthorizationHeader();  // ✅ Thêm token
            var json = JsonSerializer.Serialize(model);
            _logger.LogInformation("🔍 POST {Url}", $"{ApiBaseUrl}/Banan");
            _logger.LogInformation("📤 Data: {Data}", json);
            
            var response = await _httpClient.PostAsJsonAsync($"{ApiBaseUrl}/Banan", model);
            
            var responseContent = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("📡 Status: {Status}", response.StatusCode);
            _logger.LogInformation("📦 Response: {Response}", responseContent);
            
            response.EnsureSuccessStatusCode();
            
            try
            {
                return JsonSerializer.Deserialize<int>(responseContent, _jsonOptions);
            }
            catch
            {
                return 0;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("❌ Error CreateBanAn: {Message}", ex.Message);
            return 0;
        }
    }

    public async Task UpdateBanAn(int id, BanAnViewModel model)
    {
        try
        {
            AddAuthorizationHeader();  // ✅ Thêm token
            var response = await _httpClient.PutAsJsonAsync($"{ApiBaseUrl}/Banan/{id}", model);
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            _logger.LogError("❌ Error UpdateBanAn: {Message}", ex.Message);
            throw;
        }
    }

    public async Task DeleteBanAn(int id)
    {
        try
        {
            AddAuthorizationHeader();  // ✅ Thêm token
            var response = await _httpClient.DeleteAsync($"{ApiBaseUrl}/Banan/{id}");
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            _logger.LogError("❌ Error DeleteBanAn: {Message}", ex.Message);
            throw;
        }
    }

    // ============================================
    // MON AN
    // ============================================
    
    public async Task<List<MonAnViewModel>> GetAllMonAn()
    {
        try
        {
            AddAuthorizationHeader();  // ✅ Thêm token
            return await _httpClient.GetFromJsonAsync<List<MonAnViewModel>>($"{ApiBaseUrl}/Monan", _jsonOptions) ?? new List<MonAnViewModel>();
        }
        catch (Exception ex)
        {
            _logger.LogError("❌ Error GetAllMonAn: {Message}", ex.Message);
            return new List<MonAnViewModel>();
        }
    }

    public async Task<MonAnViewModel> GetMonAn(int id)
    {
        try
        {
            AddAuthorizationHeader();  // ✅ Thêm token
            return await _httpClient.GetFromJsonAsync<MonAnViewModel>($"{ApiBaseUrl}/Monan/{id}", _jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError("❌ Error GetMonAn: {Message}", ex.Message);
            return null;
        }
    }

    public async Task<int> CreateMonAn(MonAnViewModel model)
{
    try
    {
        AddAuthorizationHeader();
        
        // ✅ Chỉ gửi các field cần thiết, KHÔNG gửi Id
        var dto = new CreateMonAnDto
        {
            TenMon = model.TenMon,
            Gia = model.Gia,
            AnhUrl = model.AnhUrl,
            MoTa = model.MoTa,
            CoSan = model.CoSan,
            DanhMucId = model.DanhMucId
        };
        
        _logger.LogInformation("📤 Creating MonAn: {@Dto}", dto);
        
        var response = await _httpClient.PostAsJsonAsync($"{ApiBaseUrl}/Monan", dto);
        
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("❌ API Error ({StatusCode}): {Error}", 
                response.StatusCode, errorContent);
            return 0;
        }
        
        var responseContent = await response.Content.ReadAsStringAsync();
        _logger.LogInformation("✅ Created MonAn. Response: {Response}", responseContent);
        
        // API trả về ID mới tạo
        return JsonSerializer.Deserialize<int>(responseContent, _jsonOptions);
    }
    catch (Exception ex)
    {
        _logger.LogError("❌ Error CreateMonAn: {Message}", ex.Message);
        return 0;
    }
}

    public async Task UpdateMonAn(int id, MonAnViewModel model)
    {
        try
        {
            AddAuthorizationHeader();  // ✅ Thêm token
            var response = await _httpClient.PutAsJsonAsync($"{ApiBaseUrl}/Monan/{id}", model);
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            _logger.LogError("❌ Error UpdateMonAn: {Message}", ex.Message);
            throw;
        }
    }

    public async Task DeleteMonAn(int id)
    {
        try
        {
            AddAuthorizationHeader();  // ✅ Thêm token
            var response = await _httpClient.DeleteAsync($"{ApiBaseUrl}/Monan/{id}");
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            _logger.LogError("❌ Error DeleteMonAn: {Message}", ex.Message);
            throw;
        }
    }

    // ============================================
    // DON HANG
    // ============================================
    
    public async Task<List<DonHangViewModel>> GetAllDonHang()
    {
        try
        {
            AddAuthorizationHeader();  // ✅ Thêm token
            return await _httpClient.GetFromJsonAsync<List<DonHangViewModel>>($"{ApiBaseUrl}/Donhang", _jsonOptions) ?? new List<DonHangViewModel>();
        }
        catch (Exception ex)
        {
            _logger.LogError("❌ Error GetAllDonHang: {Message}", ex.Message);
            return new List<DonHangViewModel>();
        }
    }

    public async Task<DonHangViewModel> GetDonHang(int id)
    {
        try
        {
            AddAuthorizationHeader();  // ✅ Thêm token
            return await _httpClient.GetFromJsonAsync<DonHangViewModel>($"{ApiBaseUrl}/Donhang/{id}", _jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError("❌ Error GetDonHang: {Message}", ex.Message);
            return null;
        }
    }

    public async Task UpdateDonHang(int id, DonHangViewModel model)
{
    try
    {
        AddAuthorizationHeader();
        
        // Tạo DTO theo đúng format backend yêu cầu (PascalCase)
        var updateDto = new
        {
            SoDon = model.SoDon,
            BanId = model.BanId,
            TongTien = model.TongTien,
            TrangThai = model.TrangThai,
            GhiChuKhach = model.GhiChuKhach,
            NgayCapNhat = model.NgayCapNhat
        };
        
        Console.WriteLine($"📤 UpdateDonHang Request (id={id}): {JsonSerializer.Serialize(updateDto)}");
        
        var response = await _httpClient.PutAsJsonAsync($"{ApiBaseUrl}/Donhang/{id}", updateDto);
        
        // Log response để debug
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"❌ UpdateDonHang Error: {response.StatusCode} - {errorContent}");
            throw new HttpRequestException($"Update failed: {response.StatusCode} - {errorContent}");
        }
        
        Console.WriteLine($"✅ UpdateDonHang Success: {response.StatusCode}");
    }
    catch (Exception ex)
    {
        _logger.LogError("❌ Error UpdateDonHang: {Message}", ex.Message);
        throw;
    }
}
    public async Task<string> UploadImageAsync(IFormFile file)
    {
        try
        {
            AddAuthorizationHeader();

            using var content = new MultipartFormDataContent();
            using var fileStream = file.OpenReadStream();
            using var streamContent = new StreamContent(fileStream);
            
            streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
            content.Add(streamContent, "file", file.FileName);

            _logger.LogInformation("📤 Uploading image: {FileName} ({Size} bytes)", file.FileName, file.Length);

            var response = await _httpClient.PostAsync($"{ApiBaseUrl}/Upload/image", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("❌ Upload failed ({StatusCode}): {Error}", response.StatusCode, errorContent);
                throw new Exception($"Upload thất bại: {errorContent}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("✅ Upload response: {Response}", responseContent);

            // ✅ Dùng UploadImageResponse từ WebAdmin.Models
            var result = JsonSerializer.Deserialize<UploadImageResponse>(responseContent, _jsonOptions);
            
            if (result?.Success == true && !string.IsNullOrEmpty(result.Url))
            {
                _logger.LogInformation("✅ Image uploaded successfully: {Url}", result.Url);
                return result.Url;
            }

            throw new Exception("Không thể lấy URL ảnh từ response");
        }
        catch (Exception ex)
        {
            _logger.LogError("❌ Error UploadImageAsync: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<bool> DeleteImageAsync(string imageUrl)
    {
        try
        {
            if (string.IsNullOrEmpty(imageUrl))
            {
                return false;
            }

            AddAuthorizationHeader();

            _logger.LogInformation("🗑️ Deleting image: {Url}", imageUrl);

            var response = await _httpClient.DeleteAsync($"{ApiBaseUrl}/Upload/image?url={Uri.EscapeDataString(imageUrl)}");

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("✅ Image deleted successfully: {Url}", imageUrl);
                return true;
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("⚠️ Delete failed ({StatusCode}): {Error}", response.StatusCode, errorContent);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError("❌ Error DeleteImageAsync: {Message}", ex.Message);
            return false;
        }
    }
}