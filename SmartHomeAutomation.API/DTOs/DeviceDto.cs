using System.ComponentModel.DataAnnotations;

namespace SmartHomeAutomation.API.DTOs
{
    public class DeviceDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public bool IsOnline { get; set; }
        public bool IsActive { get; set; }
        public string IpAddress { get; set; }
        public string MacAddress { get; set; }
        public string FirmwareVersion { get; set; }
        public int RoomId { get; set; }
        public int UserId { get; set; }
    }

    public class CreateDeviceDto
    {
        [Required(ErrorMessage = "Cihaz adı gereklidir")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Cihaz adı 2-100 karakter arasında olmalıdır")]
        [RegularExpression(@"^[a-zA-Z0-9\s\-_çğıöşüÇĞIİÖŞÜ]+$", ErrorMessage = "Cihaz adı sadece harf, rakam, boşluk ve - _ karakterlerini içerebilir")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Cihaz tipi gereklidir")]
        [RegularExpression(@"^(LIGHT|TV|THERMOSTAT|CAMERA|SPEAKER|FAN|AC|HEATER)$", ErrorMessage = "Geçersiz cihaz tipi")]
        public string Type { get; set; }

        [RegularExpression(@"^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$", ErrorMessage = "Geçersiz IP adresi formatı")]
        public string IpAddress { get; set; }

        [RegularExpression(@"^([0-9A-Fa-f]{2}[:-]){5}([0-9A-Fa-f]{2})$", ErrorMessage = "Geçersiz MAC adresi formatı")]
        public string MacAddress { get; set; }

        [StringLength(50, ErrorMessage = "Firmware versiyonu maksimum 50 karakter olabilir")]
        public string FirmwareVersion { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Geçerli bir oda seçiniz")]
        public int RoomId { get; set; }

        public int UserId { get; set; }
    }

    public class UpdateDeviceDto
    {
        [Required(ErrorMessage = "Cihaz adı gereklidir")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Cihaz adı 2-100 karakter arasında olmalıdır")]
        [RegularExpression(@"^[a-zA-Z0-9\s\-_çğıöşüÇĞIİÖŞÜ]+$", ErrorMessage = "Cihaz adı sadece harf, rakam, boşluk ve - _ karakterlerini içerebilir")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Cihaz tipi gereklidir")]
        [RegularExpression(@"^(LIGHT|TV|THERMOSTAT|CAMERA|SPEAKER|FAN|AC|HEATER)$", ErrorMessage = "Geçersiz cihaz tipi")]
        public string Type { get; set; }

        [RegularExpression(@"^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$", ErrorMessage = "Geçersiz IP adresi formatı")]
        public string IpAddress { get; set; }

        [RegularExpression(@"^([0-9A-Fa-f]{2}[:-]){5}([0-9A-Fa-f]{2})$", ErrorMessage = "Geçersiz MAC adresi formatı")]
        public string MacAddress { get; set; }

        [StringLength(50, ErrorMessage = "Firmware versiyonu maksimum 50 karakter olabilir")]
        public string FirmwareVersion { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Geçerli bir oda seçiniz")]
        public int? RoomId { get; set; }
    }

    public class DeviceStatusDto
    {
        [Required(ErrorMessage = "Durum gereklidir")]
        [RegularExpression(@"^(ON|OFF)$", ErrorMessage = "Durum sadece ON veya OFF olabilir")]
        public string Status { get; set; }

        public bool IsOnline { get; set; }
    }
} 