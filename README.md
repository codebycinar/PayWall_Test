# PayWall.NetCore Demo

Bu proje, ASP.NET Core 6.0 kullanarak PayWall.NetCore entegrasyonunu gösteren bir demo uygulamasıdır.

## Özellikler

- Doğrudan ödeme (2D) ve 3D Secure ödeme seçenekleri
- Kart BIN sorgulama ve taksit seçenekleri
- Başarılı ve başarısız ödeme sonuç sayfaları
- PayWall ödeme servisleriyle entegrasyon

## Kurulum

1. .NET SDK 6.0 veya daha yeni bir sürümü yükleyin
2. Projeyi klonlayın
3. appsettings.json dosyasındaki API anahtarlarını gerçek PayWall kimlik bilgilerinizle güncelleyin
4. Aşağıdaki komutu çalıştırarak uygulamayı başlatın:

```bash
dotnet run
```

## Entegre Edilen PayWall Servisleri

- Direkt Ödeme (2D)
- Güvenli Ödeme (3D)
- BIN Sorgulama
- Taksit Sorgulama

## Gereksinimler

- .NET 6.0 SDK veya daha yeni
- PayWall API erişim bilgileri

## Yapılandırma

appsettings.json dosyasında PayWall bölümünü kendi kimlik bilgilerinizle güncelleyin:

```json
"PayWall": {
  "Prod": false,
  "DataCenter": "Global",
  "PublicClient": "your_public_client_here",
  "PublicKey": "your_public_key_here",
  "PrivateClient": "your_private_client_here",
  "PrivateKey": "your_private_key_here"
}
```

## Lisans

MIT