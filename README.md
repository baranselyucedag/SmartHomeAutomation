# 🏠 SmartHomeAutomation - Akıllı Ev Otomasyon Sistemi

Modern ASP.NET Core Web API ile geliştirilmiş kapsamlı bir akıllı ev otomasyon sistemi. Bu proje, güncel web teknolojileri ve en iyi güvenlik uygulamalarını kullanarak geliştirilmiştir.

## 📋 Proje Özeti

Bu sistem, kullanıcıların evlerindeki akıllı cihazları uzaktan kontrol etmelerini, otomatik senaryolar oluşturmalarını ve enerji tüketimlerini takip etmelerini sağlar. Modern yazılım geliştirme prensiplerine uygun olarak Clean Architecture yaklaşımıyla tasarlanmıştır.

**Geliştirici:** Baransel YÜCEDAĞ  
**İletişim:** baranselyucedag@gmail.com

## 🚀 Teknolojiler ve Özellikler

### 🔧 Core Teknolojiler
- **ASP.NET Core 8.0** - Modern web API framework'ü
- **Entity Framework Core** - ORM ve veritabanı yönetimi
- **SQL Server** - Ana veritabanı
- **AutoMapper** - Object mapping
- **Serilog** - Yapılandırılabilir loglama
- **JWT Authentication** - Güvenli kimlik doğrulama
- **Docker** - Konteynerizasyon desteği

### 🛡️ Güvenlik Özellikleri
- **JWT Token Authentication** - Güvenli API erişimi
- **CSRF Protection** - Cross-Site Request Forgery koruması
- **SQL Injection Protection** - Veritabanı güvenliği
- **XSS Protection** - Cross-Site Scripting koruması
- **Rate Limiting** - API isteklerini sınırlama (100 req/dk)
- **HTTPS Enforcement** - Güvenli iletişim
- **Security Headers** - Kapsamlı güvenlik başlıkları

### ⚡ Performans ve Ölçeklenebilirlik
- **Memory Caching** - Hızlı veri erişimi
- **Asynchronous Programming** - Yüksek performans
- **API Versioning** - Geriye uyumlu API geliştirme
- **Health Checks** - Sistem sağlığı izleme
- **Connection Pooling** - Veritabanı performansı

### 🧪 Test ve Kalite
- **Unit Tests** - Birim testleri
- **Integration Tests** - Entegrasyon testleri
- **Mocking** - Test izolasyonu
- **Code Coverage** - Test kapsamı

## 🏗️ Mimari Yapı

```
📁 SmartHomeAutomation/
├── 📁 SmartHomeAutomation.API/          # Web API katmanı
│   ├── 📁 Controllers/                   # API kontrolcüleri
│   ├── 📁 DTOs/                         # Veri transfer nesneleri
│   ├── 📁 Services/                     # İş mantığı servisleri
│   ├── 📁 Middleware/                   # Özel middleware'ler
│   ├── 📁 HealthChecks/                 # Sistem sağlığı kontrolleri
│   └── 📁 Validators/                   # Veri doğrulama kuralları
├── 📁 SmartHomeAutomation.Core/         # Domain katmanı
│   ├── 📁 Entities/                     # Varlık sınıfları
│   └── 📁 Interfaces/                   # Arayüz tanımları
├── 📁 SmartHomeAutomation.Infrastructure/ # Veri erişim katmanı
│   ├── 📁 Data/                         # DbContext ve UnitOfWork
│   └── 📁 Repositories/                 # Repository implementasyonları
└── 📁 SmartHomeAutomation.Tests/        # Test projesi
    ├── 📁 UnitTests/                    # Birim testleri
    └── 📁 IntegrationTests/             # Entegrasyon testleri
```

## 📊 Temel Özellikler

### 🔌 Cihaz Yönetimi
- Akıllı cihaz ekleme, düzenleme, silme
- Cihaz durumu kontrolü (açık/kapalı)
- Enerji tüketimi takibi
- Cihaz kategorileri (aydınlatma, güvenlik, iklim vb.)
- Gerçek zamanlı durum güncellemeleri

### 🏠 Oda Yönetimi
- Oda oluşturma ve düzenleme
- Cihazları odalara atama
- Kat bazında organizasyon
- Oda bazında toplu kontrol

### 🎬 Senaryo Sistemi
- Özel senaryolar oluşturma
- Çoklu cihaz kontrolü
- Zamanlama ve otomasyon
- Hazır senaryo şablonları
- Koşullu senaryolar

### 📈 Enerji Takibi
- Günlük/aylık enerji raporları
- Cihaz bazında tüketim analizi
- Maliyet hesaplama
- Grafik ve istatistikler

### 👥 Kullanıcı Yönetimi
- Güvenli kayıt ve giriş sistemi
- Profil yönetimi
- Şifre değiştirme
- Çoklu kullanıcı desteği

## 🔧 Kurulum ve Çalıştırma

### Gereksinimler
- .NET 8.0 SDK
- SQL Server (LocalDB desteklenir)
- Visual Studio 2022 veya VS Code
- Git

### 1. Projeyi Klonlama
```bash
git clone https://github.com/baranselyucedag/SmartHomeAutomation.git
cd SmartHomeAutomation
```

### 2. Veritabanı Kurulumu
```bash
cd SmartHomeAutomation.API
dotnet ef database update
```

### 3. Projeyi Çalıştırma
```bash
dotnet run --project SmartHomeAutomation.API
```

### 4. Docker ile Çalıştırma
```bash
docker-compose up -d
```

## 📡 API Endpoints

### 🔐 Kimlik Doğrulama
```
POST /api/auth/login     # Kullanıcı girişi
POST /api/auth/register  # Kullanıcı kaydı
POST /api/auth/refresh   # Token yenileme
```

### 🔌 Cihaz İşlemleri
```
GET    /api/device           # Tüm cihazları listele
GET    /api/device/{id}      # Belirli cihazı getir
POST   /api/device          # Yeni cihaz ekle
PUT    /api/device/{id}     # Cihaz güncelle
DELETE /api/device/{id}     # Cihaz sil
POST   /api/device/{id}/toggle # Cihaz durumunu değiştir
```

### 🏠 Oda İşlemleri
```
GET    /api/room            # Tüm odaları listele
POST   /api/room           # Yeni oda ekle
PUT    /api/room/{id}      # Oda güncelle
DELETE /api/room/{id}      # Oda sil
```

### 🎬 Senaryo İşlemleri
```
GET    /api/scene               # Senaryoları listele
POST   /api/scene              # Yeni senaryo oluştur
POST   /api/scene/{id}/execute # Senaryoyu çalıştır
```

### 🔍 Sistem İzleme
```
GET /health              # Sistem sağlığı
GET /api/security/report # Güvenlik raporu
```

## 🛡️ Güvenlik Uygulamaları

### API Güvenliği
- **JWT Token Tabanlı Kimlik Doğrulama**: Stateless ve güvenli
- **Role-Based Authorization**: Yetki bazlı erişim kontrolü
- **Request Rate Limiting**: DDoS koruması
- **Input Validation**: Tüm girişler doğrulanır

### Veri Güvenliği
- **SQL Injection Koruması**: Parametreli sorgular
- **XSS Koruması**: Çıktı kodlaması
- **CSRF Koruması**: Token tabanlı koruma
- **Data Encryption**: Hassas veriler şifrelenir

### İletişim Güvenliği
- **HTTPS Zorunluluğu**: TLS 1.2+ protokolü
- **Security Headers**: HSTS, CSP, X-Frame-Options
- **CORS Politikaları**: Kontrollü kaynak paylaşımı

## 📊 Performans Optimizasyonları

### Caching Stratejileri
- **Memory Caching**: Sık kullanılan veriler
- **Response Caching**: API yanıtları
- **Query Optimization**: Veritabanı sorguları

### Asenkron İşlemler
- **Async/Await Pattern**: Tüm I/O işlemleri
- **Background Services**: Uzun süren işlemler
- **Event-Driven Architecture**: Gevşek bağlantı

## 🧪 Test Stratejisi

### Test Türleri
- **Unit Tests**: İş mantığı testleri
- **Integration Tests**: API endpoint testleri
- **Performance Tests**: Yük ve stres testleri
- **Security Tests**: Güvenlik açığı testleri

### Test Araçları
- **xUnit**: Test framework'ü
- **Moq**: Mocking kütüphanesi
- **FluentAssertions**: Test doğrulamaları
- **WebApplicationFactory**: Entegrasyon testleri

## 📈 İzleme ve Loglama

### Loglama
- **Serilog**: Yapılandırılabilir loglama
- **Structured Logging**: JSON formatında loglar
- **Log Levels**: Debug, Info, Warning, Error
- **Log Enrichment**: Kullanıcı ve istek bilgileri

### Metriks
- **Health Checks**: Sistem sağlığı
- **Performance Counters**: Performans metrikleri
- **Error Tracking**: Hata izleme
- **Usage Analytics**: Kullanım istatistikleri

## 🚀 Gelecek Planları

### Yeni Özellikler
- [ ] **Mobile App**: iOS/Android uygulaması
- [ ] **Voice Control**: Sesli komut desteği
- [ ] **Machine Learning**: Akıllı otomasyon önerileri
- [ ] **IoT Integration**: Daha fazla cihaz desteği
- [ ] **Real-time Notifications**: Anlık bildirimler

### Teknik İyileştirmeler
- [ ] **Microservices**: Mikroservis mimarisine geçiş
- [ ] **Event Sourcing**: Olay tabanlı veri modeli
- [ ] **GraphQL**: Esnek API sorguları
- [ ] **Kubernetes**: Orkestrasyon desteği
- [ ] **Observability**: Kapsamlı izleme

---

⭐ Bu projeyi beğendiyseniz, lütfen yıldız verin! 