// API endpoint'leri
const API = {
    rooms: 'http://localhost:5292/api/room',
    devices: 'http://localhost:5292/api/device',
    scenes: 'http://localhost:5292/api/scene',
    user: 'http://localhost:5292/api/user'
};

// API çağrıları için yardımcı fonksiyon
async function fetchAPI(endpoint, options = {}) {
    try {
        // JWT token'ı localStorage'dan al
        const token = localStorage.getItem('token');
        
        const response = await fetch(endpoint, {
            ...options,
            headers: {
                'Content-Type': 'application/json',
                ...(token && { 'Authorization': `Bearer ${token}` }),
                ...options.headers
            }
        });

        if (!response.ok) {
            // 401 hatası durumunda login sayfasına yönlendir
            if (response.status === 401) {
                localStorage.removeItem('token');
                localStorage.removeItem('user');
                window.location.href = '/login.html';
                return;
            }
            
            // Hata mesajını response body'den okumaya çalış
            let errorMessage = `HTTP error! status: ${response.status}`;
            try {
                const errorData = await response.json();
                if (errorData.message) {
                    errorMessage = errorData.message;
                } else if (errorData.Message) {
                    errorMessage = errorData.Message;
                }
            } catch (e) {
                // JSON parse hatası varsa default mesajı kullan
            }
            
            throw new Error(errorMessage);
        }

        // 204 No Content için boş response kontrolü
        if (response.status === 204 || response.headers.get('content-length') === '0') {
            return null;
        }
        
        // Content-Type kontrolü
        const contentType = response.headers.get('content-type');
        if (contentType && contentType.includes('application/json')) {
            return await response.json();
        }
        
        return null;
    } catch (error) {
        console.error('API Error:', error);
        throw error;
    }
}

document.addEventListener('DOMContentLoaded', async function() {
    // Check if user is logged in
    const token = localStorage.getItem('token');
    const user = localStorage.getItem('user');
    
    if (!token || !user) {
        // Redirect to login page if not logged in
        window.location.href = '/login.html';
        return;
    }
    
    // Update user info in the nav
    try {
        const userData = JSON.parse(user);
        const userInfo = document.querySelector('.user-info span');
        if (userInfo) {
            userInfo.textContent = userData.firstName || userData.username;
        }
        
        const userAvatar = document.querySelector('.user-info img');
        if (userAvatar) {
            userAvatar.src = `https://ui-avatars.com/api/?name=${encodeURIComponent(userData.firstName + ' ' + userData.lastName)}&background=667eea&color=fff`;
        }
    } catch (error) {
        console.error('Error parsing user data:', error);
    }
    
    // Sayfa yüklendiğinde verileri getir
    try {
        await loadDashboard();
    } catch (error) {
        console.error('Error loading dashboard:', error);
    }

    // Device toggle functionality for room devices
    document.addEventListener('click', async function(e) {
        if (e.target.closest('.device-item')) {
            const deviceItem = e.target.closest('.device-item');
            const deviceId = deviceItem.dataset.deviceId;
            const currentState = deviceItem.classList.contains('active');

            console.log('Device item clicked:', {deviceId, currentState}); // Debug

            try {
                await fetchAPI(`${API.devices}/${deviceId}/toggle`, {
                    method: 'POST',
                    body: JSON.stringify({ state: !currentState })
                });

                deviceItem.classList.toggle('active');
                const deviceName = deviceItem.querySelector('span').textContent;
                const isActive = deviceItem.classList.contains('active');
                console.log(`${deviceName} is now ${isActive ? 'ON' : 'OFF'}`);
                
                // İstatistikleri güncelle
                await updateStats();
            } catch (error) {
                console.error('Error toggling device:', error);
            }
        }
    });

    // Device toggle functionality for device cards (new section)
    document.addEventListener('click', async function(e) {
        if (e.target.classList.contains('toggle-btn')) {
            const toggleBtn = e.target;
            const deviceId = toggleBtn.dataset.deviceId;
            
            console.log('Toggle button clicked:', {deviceId}); // Debug

            if (!deviceId || deviceId === 'undefined') {
                console.error('Device ID is undefined');
                alert('Cihaz ID\'si geçersiz');
                return;
            }

            // Visual feedback
            const originalText = toggleBtn.textContent;
            toggleBtn.textContent = 'İşleniyor...';
            toggleBtn.disabled = true;

            try {
                const response = await fetchAPI(`${API.devices}/${deviceId}/toggle`, {
                    method: 'POST'
                });

                console.log('Device toggle response:', response);

                // Update button state
                toggleBtn.classList.toggle('active');
                const isActive = toggleBtn.classList.contains('active');
                toggleBtn.textContent = isActive ? 'Kapat' : 'Aç';

                // Update device status indicator
                const deviceCard = toggleBtn.closest('.device-card');
                const statusElement = deviceCard.querySelector('.device-status');
                if (statusElement) {
                    statusElement.classList.toggle('active');
                    statusElement.classList.toggle('inactive');
                    statusElement.textContent = isActive ? 'Aktif' : 'Pasif';
                }

                console.log(`Device ${deviceId} toggled successfully`);
                
                // İstatistikleri güncelle
                await updateStats();
            } catch (error) {
                console.error('Error toggling device:', error);
                alert(`Cihaz durumu değiştirilemedi: ${error.message}`);
            } finally {
                toggleBtn.disabled = false;
                if (toggleBtn.textContent === 'İşleniyor...') {
                    toggleBtn.textContent = originalText;
                }
            }
        }
    });

    // Scene trigger functionality
    document.addEventListener('click', async function(e) {
        if (e.target.classList.contains('scene-trigger')) {
            const trigger = e.target;
            
            // Birden fazla yolla scene ID'yi almaya çalışalım
            let sceneId = trigger.getAttribute('data-scene-id') || 
                         trigger.dataset.sceneId || 
                         trigger.getAttribute('data-sceneid');
            
            const sceneName = trigger.parentElement.querySelector('h3') ? 
                             trigger.parentElement.querySelector('h3').textContent : 
                             'Unknown Scene';

            // Debug ekleyelim
            console.log('Scene trigger clicked:', {
                sceneId: sceneId,
                sceneName: sceneName,
                attributes: Array.from(trigger.attributes).map(attr => `${attr.name}="${attr.value}"`),
                classList: Array.from(trigger.classList),
                innerHTML: trigger.innerHTML,
                outerHTML: trigger.outerHTML.substring(0, 200)
            });

            // sceneId undefined kontrolü
            if (!sceneId || sceneId === 'undefined' || sceneId === 'null') {
                console.error('Scene ID is undefined or invalid:', sceneId);
                alert('Senaryo ID\'si geçersiz. Lütfen sayfayı yenileyin.');
                return;
            }

            // Visual feedback
            const originalText = trigger.textContent;
            trigger.textContent = 'Çalıştırılıyor...';
            trigger.disabled = true;

            try {
                console.log(`Executing scene with ID: ${sceneId}`);
                await fetchAPI(`${API.scenes}/${sceneId}/execute`, {
                    method: 'POST'
                });
                console.log(`Executed scene: ${sceneName}`);
                alert(`${sceneName} senaryosu başarıyla çalıştırıldı!`);
            } catch (error) {
                console.error('Error executing scene:', error);
                alert(`Senaryo çalıştırılırken hata oluştu: ${error.message}`);
            } finally {
                setTimeout(() => {
                    trigger.textContent = originalText;
                    trigger.disabled = false;
                }, 2000);
            }
        }
    });

    // Navigation functionality
    const navLinks = document.querySelectorAll('.nav-links a');
    navLinks.forEach(link => {
        link.addEventListener('click', function(e) {
            e.preventDefault();
            
            // Update active nav item
            navLinks.forEach(l => l.parentElement.classList.remove('active'));
            this.parentElement.classList.add('active');
            
            // Update header title
            const headerTitle = document.querySelector('header h1');
            const pageTitle = this.querySelector('span').textContent;
            headerTitle.textContent = pageTitle;
            
            // Show/hide page content
            showPage(pageTitle);
        });
    });

    // Modal functionality
    setupModals();
    
    // Logout functionality
    setupLogout();
    
    // Setup form submissions
    setupFormSubmissions();
    
    // Setup refresh button
    document.querySelector('.refresh-btn').addEventListener('click', loadDashboard);
});

// Setup modal functionality
function setupModals() {
    const addRoomBtn = document.getElementById('add-room-btn');
    const addRoomBtn2 = document.getElementById('add-room-btn-2');
    const addSceneBtn = document.getElementById('add-scene-btn');
    const addSceneBtn2 = document.getElementById('add-scene-btn-2');
    const addDeviceBtn = document.getElementById('add-device-btn');
    const changePasswordBtn = document.getElementById('change-password-btn');
    const roomModal = document.getElementById('add-room-modal');
    const sceneModal = document.getElementById('add-scene-modal');
    const deviceModal = document.getElementById('add-device-modal');
    const changePasswordModal = document.getElementById('change-password-modal');
    
    // Edit modals
    const editDeviceModal = document.getElementById('edit-device-modal');
    const editRoomModal = document.getElementById('edit-room-modal');
    const editSceneModal = document.getElementById('edit-scene-modal');
    
    const closeButtons = document.querySelectorAll('.close, .cancel-btn');
    
    // Debug - element kontrolü
    console.log('Modal setup - Elements found:', {
        addRoomBtn: !!addRoomBtn,
        addSceneBtn: !!addSceneBtn,
        addDeviceBtn: !!addDeviceBtn,
        roomModal: !!roomModal,
        sceneModal: !!sceneModal,
        deviceModal: !!deviceModal
    });
    
    // Open room modal
    if (addRoomBtn && roomModal) {
        addRoomBtn.addEventListener('click', () => {
            console.log('Room modal opening...'); // Debug
            roomModal.style.display = 'block';
        });
    }
    
    if (addRoomBtn2 && roomModal) {
        addRoomBtn2.addEventListener('click', () => {
            console.log('Room modal opening from rooms page...'); // Debug
            roomModal.style.display = 'block';
        });
    }
    
    // Open scene modal
    if (addSceneBtn && sceneModal) {
        addSceneBtn.addEventListener('click', () => {
            console.log('Scene modal opening...'); // Debug
            sceneModal.style.display = 'block';
        });
    }
    
    if (addSceneBtn2 && sceneModal) {
        addSceneBtn2.addEventListener('click', () => {
            console.log('Scene modal opening from scenes page...'); // Debug
            sceneModal.style.display = 'block';
        });
    }
    
    // Open device modal
    if (addDeviceBtn && deviceModal) {
        addDeviceBtn.addEventListener('click', async () => {
            console.log('Device modal opening...'); // Debug
            await loadRoomsToSelect();
            deviceModal.style.display = 'block';
        });
    }

    // Open change password modal
    if (changePasswordBtn && changePasswordModal) {
        changePasswordBtn.addEventListener('click', () => {
            console.log('Change password modal opening...'); // Debug
            showChangePasswordModal();
        });
    }
    
    // Close modals
    closeButtons.forEach(btn => {
        btn.addEventListener('click', () => {
            if (roomModal) roomModal.style.display = 'none';
            if (sceneModal) sceneModal.style.display = 'none';
            if (deviceModal) deviceModal.style.display = 'none';
            if (changePasswordModal) changePasswordModal.style.display = 'none';
            if (editDeviceModal) editDeviceModal.style.display = 'none';
            if (editRoomModal) editRoomModal.style.display = 'none';
            if (editSceneModal) editSceneModal.style.display = 'none';
        });
    });
    
    // Close modals when clicking outside content
    window.addEventListener('click', (e) => {
        if (e.target === roomModal) {
            roomModal.style.display = 'none';
        }
        if (e.target === sceneModal) {
            sceneModal.style.display = 'none';
        }
        if (e.target === deviceModal) {
            deviceModal.style.display = 'none';
        }
        if (e.target === changePasswordModal) {
            changePasswordModal.style.display = 'none';
        }
        if (e.target === editDeviceModal) {
            editDeviceModal.style.display = 'none';
        }
        if (e.target === editRoomModal) {
            editRoomModal.style.display = 'none';
        }
        if (e.target === editSceneModal) {
            editSceneModal.style.display = 'none';
        }
    });
}

// Setup form submissions
function setupFormSubmissions() {
    const addRoomForm = document.getElementById('add-room-form');
    const addSceneForm = document.getElementById('add-scene-form');
    const addDeviceForm = document.getElementById('add-device-form');
    const changePasswordForm = document.getElementById('change-password-form');
    
    // Null check ekleyelim
    if (!addRoomForm || !addSceneForm || !addDeviceForm) {
        console.error('Forms not found:', {
            addRoomForm: !!addRoomForm,
            addSceneForm: !!addSceneForm,
            addDeviceForm: !!addDeviceForm
        });
        return;
    }
    
    // Room form submission
    addRoomForm.addEventListener('submit', async (e) => {
        e.preventDefault();
        
        const formData = {
            name: document.getElementById('room-name').value,
            description: document.getElementById('room-description').value,
            floor: parseInt(document.getElementById('room-floor').value, 10)
        };
        
        console.log('Sending room data:', formData);
        
        try {
            const result = await fetchAPI(API.rooms, {
                method: 'POST',
                body: JSON.stringify(formData)
            });
            
            console.log('Room creation result:', result);
            
            // Close modal and reset form
            document.getElementById('add-room-modal').style.display = 'none';
            addRoomForm.reset();
            
            // Refresh rooms
            await loadDashboard();
            
            // Show success message
            alert('Oda başarıyla eklendi!');
        } catch (error) {
            console.error('Error adding room:', error);
            alert('Oda eklenirken bir hata oluştu: ' + error.message);
        }
    });
    
    // Scene form submission
    addSceneForm.addEventListener('submit', async (e) => {
        e.preventDefault();
        
        const formData = {
            name: document.getElementById('scene-name').value,
            description: document.getElementById('scene-description').value,
            icon: document.getElementById('scene-icon').value,
            sceneDevices: [] // This would need to be populated with selected devices
        };
        
        try {
            await fetchAPI(API.scenes, {
                method: 'POST',
                body: JSON.stringify(formData)
            });
            
            // Close modal and reset form
            document.getElementById('add-scene-modal').style.display = 'none';
            addSceneForm.reset();
            
            // Refresh scenes
            await loadDashboard();
            
            // Show success message
            alert('Senaryo başarıyla eklendi!');
        } catch (error) {
            console.error('Error adding scene:', error);
            alert('Senaryo eklenirken bir hata oluştu!');
        }
    });

    // Device form submission
    addDeviceForm.addEventListener('submit', async (e) => {
        e.preventDefault();
        
        const formData = {
            name: document.getElementById('device-name').value,
            type: document.getElementById('device-type').value,
            roomId: parseInt(document.getElementById('device-room').value, 10),
            ipAddress: document.getElementById('device-ip').value || null,
            macAddress: document.getElementById('device-mac').value || null,
            firmwareVersion: "1.0.0" // Default değer
        };
        
        console.log('Sending device data:', formData);
        
        try {
            const result = await fetchAPI(API.devices, {
                method: 'POST',
                body: JSON.stringify(formData)
            });
            
            console.log('Device creation result:', result);
            
            // Close modal and reset form
            document.getElementById('add-device-modal').style.display = 'none';
            addDeviceForm.reset();
            
            // Refresh dashboard
            await loadDashboard();
            
            // Show success message
            alert('Cihaz başarıyla eklendi!');
        } catch (error) {
            console.error('Error adding device:', error);
            alert('Cihaz eklenirken bir hata oluştu: ' + error.message);
        }
    });

    // Change password form submission
    if (changePasswordForm) {
        changePasswordForm.addEventListener('submit', async (e) => {
            e.preventDefault();
            
            const oldPassword = document.getElementById('old-password').value;
            const newPassword = document.getElementById('new-password').value;
            const confirmPassword = document.getElementById('confirm-password').value;
            
            // Client-side validation
            if (newPassword !== confirmPassword) {
                showNotification('Yeni şifreler eşleşmiyor', 'error');
                return;
            }
            
            if (newPassword.length < 6) {
                showNotification('Yeni şifre en az 6 karakter olmalı', 'error');
                return;
            }
            
            const formData = {
                oldPassword: oldPassword,
                newPassword: newPassword,
                confirmNewPassword: confirmPassword
            };
            
            await changePassword(formData);
        });
    }
    
    // Edit form submissions
    const editDeviceForm = document.getElementById('edit-device-form');
    const editRoomForm = document.getElementById('edit-room-form');
    const editSceneForm = document.getElementById('edit-scene-form');
    
    // Edit Device form submission
    if (editDeviceForm) {
        editDeviceForm.addEventListener('submit', async (e) => {
            e.preventDefault();
            
            const deviceId = document.getElementById('edit-device-id').value;
            const formData = {
                id: parseInt(deviceId),
                name: document.getElementById('edit-device-name').value,
                type: document.getElementById('edit-device-type').value,
                roomId: parseInt(document.getElementById('edit-device-room').value, 10),
                ipAddress: document.getElementById('edit-device-ip').value || null,
                macAddress: document.getElementById('edit-device-mac').value || null,
                firmwareVersion: "1.0.0" // Default değer
            };
            
            try {
                await fetchAPI(`${API.devices}/${deviceId}`, {
                    method: 'PUT',
                    body: JSON.stringify(formData)
                });
                
                // Close modal and refresh
                document.getElementById('edit-device-modal').style.display = 'none';
                await loadDashboard();
                showNotification('Cihaz başarıyla güncellendi!', 'success');
            } catch (error) {
                console.error('Error updating device:', error);
                showNotification('Cihaz güncellenirken hata oluştu: ' + error.message, 'error');
            }
        });
    }
    
    // Edit Room form submission
    if (editRoomForm) {
        editRoomForm.addEventListener('submit', async (e) => {
            e.preventDefault();
            
            const roomId = document.getElementById('edit-room-id').value;
            const formData = {
                id: parseInt(roomId),
                name: document.getElementById('edit-room-name').value,
                description: document.getElementById('edit-room-description').value,
                floor: parseInt(document.getElementById('edit-room-floor').value, 10)
            };
            
            try {
                await fetchAPI(`${API.rooms}/${roomId}`, {
                    method: 'PUT',
                    body: JSON.stringify(formData)
                });
                
                // Close modal and refresh
                document.getElementById('edit-room-modal').style.display = 'none';
                await loadDashboard();
                loadRoomsPage(); // Eğer rooms sayfasındaysak onu da yenile
                showNotification('Oda başarıyla güncellendi!', 'success');
            } catch (error) {
                console.error('Error updating room:', error);
                showNotification('Oda güncellenirken hata oluştu: ' + error.message, 'error');
            }
        });
    }
    
    // Edit Scene form submission
    if (editSceneForm) {
        editSceneForm.addEventListener('submit', async (e) => {
            e.preventDefault();
            
            const sceneId = document.getElementById('edit-scene-id').value;
            const formData = {
                id: parseInt(sceneId),
                name: document.getElementById('edit-scene-name').value,
                description: document.getElementById('edit-scene-description').value,
                icon: document.getElementById('edit-scene-icon').value,
                sceneDevices: [] // Bu kısım daha sonra geliştirilebilir
            };
            
            try {
                await fetchAPI(`${API.scenes}/${sceneId}`, {
                    method: 'PUT',
                    body: JSON.stringify(formData)
                });
                
                // Close modal and refresh
                document.getElementById('edit-scene-modal').style.display = 'none';
                await loadDashboard();
                loadScenesPage(); // Eğer scenes sayfasındaysak onu da yenile
                showNotification('Senaryo başarıyla güncellendi!', 'success');
            } catch (error) {
                console.error('Error updating scene:', error);
                showNotification('Senaryo güncellenirken hata oluştu: ' + error.message, 'error');
            }
        });
    }

    // Setup event listeners for edit/delete buttons
    setupEditDeleteListeners();
}

// Düzenleme ve silme butonları için event listener'ları kur
function setupEditDeleteListeners() {
    // Event delegation kullanarak dinamik olarak eklenen butonları yakala
    document.addEventListener('click', function(e) {
        // Cihaz düzenleme
        if (e.target.closest('.edit-btn[data-device-id]')) {
            e.preventDefault();
            const deviceId = e.target.closest('.edit-btn').getAttribute('data-device-id');
            editDevice(parseInt(deviceId));
        }
        
        // Cihaz silme
        if (e.target.closest('.delete-btn[data-device-id]')) {
            e.preventDefault();
            const deviceId = e.target.closest('.delete-btn').getAttribute('data-device-id');
            deleteDevice(parseInt(deviceId));
        }
        
        // Oda düzenleme
        if (e.target.closest('.edit-btn[data-room-id]')) {
            e.preventDefault();
            const roomId = e.target.closest('.edit-btn').getAttribute('data-room-id');
            editRoom(parseInt(roomId));
        }
        
        // Oda silme
        if (e.target.closest('.delete-btn[data-room-id]')) {
            e.preventDefault();
            const roomId = e.target.closest('.delete-btn').getAttribute('data-room-id');
            deleteRoom(parseInt(roomId));
        }
        
        // Senaryo düzenleme
        if (e.target.closest('.edit-btn[data-scene-id]')) {
            e.preventDefault();
            const sceneId = e.target.closest('.edit-btn').getAttribute('data-scene-id');
            editScene(parseInt(sceneId));
        }
        
        // Senaryo silme
        if (e.target.closest('.delete-btn[data-scene-id]')) {
            e.preventDefault();
            const sceneId = e.target.closest('.delete-btn').getAttribute('data-scene-id');
            deleteScene(parseInt(sceneId));
        }
    });
}

// Dashboard verilerini yükle
async function loadDashboard() {
    try {
        console.log('Loading dashboard data...'); // Debug
        
        // Odaları yükle
        const rooms = await fetchAPI(API.rooms);
        console.log('Loaded rooms:', rooms); // Debug
        updateRoomsOverview(rooms);

        // Cihazları yükle
        const devices = await fetchAPI(API.devices);
        console.log('Loaded devices:', devices); // Debug
        updateDevicesOverview(devices);

        // Senaryoları yükle
        const scenes = await fetchAPI(API.scenes);
        console.log('Loaded scenes:', scenes); // Debug
        updateScenesOverview(scenes);

        // İstatistikleri güncelle
        updateStats();
    } catch (error) {
        console.error('Error loading dashboard data:', error);
    }
}

// Odaları device select'e yükle
async function loadRoomsToSelect() {
    try {
        const rooms = await fetchAPI(API.rooms);
        console.log('Loading rooms to select:', rooms);
        
        const roomSelect = document.getElementById('device-room');
        roomSelect.innerHTML = '<option value="">Oda seçiniz</option>';
        
        if (rooms && rooms.length > 0) {
            rooms.forEach(room => {
                const option = document.createElement('option');
                option.value = room.Id;
                option.textContent = room.Name || 'Adsız Oda';
                roomSelect.appendChild(option);
            });
        }
    } catch (error) {
        console.error('Error loading rooms to select:', error);
    }
}

async function loadRoomsToEditSelect() {
    try {
        const rooms = await fetchAPI(API.rooms);
        console.log('Loading rooms to edit select:', rooms);
        
        const roomSelect = document.getElementById('edit-device-room');
        roomSelect.innerHTML = '<option value="">Oda seçiniz</option>';
        
        if (rooms && rooms.length > 0) {
            rooms.forEach(room => {
                const option = document.createElement('option');
                option.value = room.Id;
                option.textContent = room.Name || 'Adsız Oda';
                roomSelect.appendChild(option);
            });
        }
    } catch (error) {
        console.error('Error loading rooms to edit select:', error);
    }
}

// Oda kartlarını güncelle
function updateRoomsOverview(rooms) {
    console.log('Updating rooms overview with:', rooms);
    console.log('Rooms type:', typeof rooms);
    console.log('Rooms array length:', Array.isArray(rooms) ? rooms.length : 'Not an array');
    
    const roomCardsContainer = document.querySelector('.room-cards');
    if (!roomCardsContainer) {
        console.error('Room cards container not found!');
        return;
    }
    
    if (!rooms || rooms.length === 0) {
        console.log('No rooms found');
        roomCardsContainer.innerHTML = '<div class="empty-state">Henüz oda bulunmuyor</div>';
        return;
    }
    
    roomCardsContainer.innerHTML = rooms.map((room, index) => {
        console.log(`Processing room ${index}:`, room);
        console.log(`Room keys:`, Object.keys(room));
        
        // API PascalCase döndürüyor, bu yüzden büyük harfle başlayan field'ları kullanıyoruz
        const roomId = room.Id;
        const roomName = room.Name || 'Adsız Oda';
        const roomDevices = room.Devices || [];
        const deviceCount = roomDevices.length;
        
        console.log(`Room ${index} processed:`, { roomId, roomName, deviceCount });
        
        return `
            <div class="room-card" data-room-id="${roomId}">
                <div class="room-header">
                    <h3>${roomName}</h3>
                    <span class="device-count">${deviceCount} Cihaz</span>
                    <div class="room-actions">
                        <button class="edit-btn" data-room-id="${roomId}" title="Düzenle">
                            <i class="fas fa-edit"></i>
                        </button>
                        <button class="delete-btn" data-room-id="${roomId}" title="Sil">
                            <i class="fas fa-trash"></i>
                        </button>
                    </div>
                </div>
                <div class="room-devices">
                    ${roomDevices.length > 0 ? roomDevices.map((device, deviceIndex) => {
                        console.log(`Processing device ${deviceIndex}:`, device);
                        const deviceId = device.Id;
                        const deviceName = device.Name || 'Adsız Cihaz';
                        const deviceType = device.Type || 'UNKNOWN';
                        const isActive = device.Status === 'ON' || device.IsActive;
                        
                        return `
                            <div class="device-item ${isActive ? 'active' : ''}" data-device-id="${deviceId}">
                                <i class="fas fa-${getDeviceIcon(deviceType)}"></i>
                                <span>${deviceName}</span>
                            </div>
                        `;
                    }).join('') : '<div class="empty-device">Bu odada cihaz bulunmuyor</div>'}
                </div>
            </div>
        `;
    }).join('');
}

// Cihaz kartlarını güncelle
function updateDevicesOverview(devices) {
    console.log('Updating devices overview with:', devices);
    
    const deviceCardsContainer = document.querySelector('.device-cards');
    if (!devices || devices.length === 0) {
        console.log('No devices found');
        deviceCardsContainer.innerHTML = '<div class="empty-state">Henüz cihaz bulunmuyor</div>';
        return;
    }
    
    deviceCardsContainer.innerHTML = devices.map(device => {
        console.log('Processing device:', device);
        // API PascalCase döndürüyor
        const deviceName = device.Name || 'Adsız Cihaz';
        const isActive = device.Status === 'ON' || device.IsActive;
        
        return `
            <div class="device-card">
                <div class="device-header">
                    <h3>${deviceName}</h3>
                    <span class="device-status ${isActive ? 'active' : 'inactive'}">
                        ${isActive ? 'Aktif' : 'Pasif'}
                    </span>
                </div>
                <div class="device-info">
                    <p><i class="fas fa-${getDeviceIcon(device.Type)}"></i> ${device.Type}</p>
                    <p><i class="fas fa-network-wired"></i> ${device.IpAddress || 'IP Yok'}</p>
                </div>
                <div class="device-actions">
                    <button class="toggle-btn ${isActive ? 'active' : ''}" data-device-id="${device.Id}">
                        ${isActive ? 'Kapat' : 'Aç'}
                    </button>
                    <button class="edit-btn" data-device-id="${device.Id}" title="Düzenle">
                        <i class="fas fa-edit"></i>
                    </button>
                    <button class="delete-btn" data-device-id="${device.Id}" title="Sil">
                        <i class="fas fa-trash"></i>
                    </button>
                </div>
            </div>
        `;
    }).join('');
}

// Senaryo kartlarını güncelle
function updateScenesOverview(scenes) {
    console.log('=== updateScenesOverview START ===');
    console.log('Updating scenes overview with:', scenes);
    console.log('scenes type:', typeof scenes);
    console.log('scenes length:', scenes ? scenes.length : 'null/undefined');
    
    const sceneCardsContainer = document.querySelector('.scene-cards');
    if (!scenes || scenes.length === 0) {
        console.log('No scenes found');
        sceneCardsContainer.innerHTML = '<div class="empty-state">Henüz senaryo bulunmuyor</div>';
        return;
    }
    
    sceneCardsContainer.innerHTML = scenes.map((scene, index) => {
        console.log(`=== Processing scene ${index} ===`);
        console.log('Full scene object:', scene);
        console.log('scene.Id:', scene.Id);
        console.log('scene.Name:', scene.Name);
        console.log('Object.keys(scene):', Object.keys(scene));
        
        // API PascalCase döndürüyor
        const sceneId = scene.Id;
        const sceneName = scene.Name || 'Adsız Senaryo';
        
        console.log(`Final values for scene ${index}:`, {sceneId, sceneName});
        
        const htmlTemplate = `
            <div class="scene-card">
                <i class="fas fa-${getSceneIcon(sceneName)}"></i>
                <h3>${sceneName}</h3>
                <div class="scene-actions">
                    <button class="scene-trigger" data-scene-id="${sceneId}">Çalıştır</button>
                    <button class="edit-btn" data-scene-id="${sceneId}" title="Düzenle">
                        <i class="fas fa-edit"></i>
                    </button>
                    <button class="delete-btn" data-scene-id="${sceneId}" title="Sil">
                        <i class="fas fa-trash"></i>
                    </button>
                </div>
            </div>
        `;
        
        console.log(`HTML template for scene ${index}:`, htmlTemplate);
        return htmlTemplate;
    }).join('');
    
    console.log('=== updateScenesOverview END ===');
}

// İstatistikleri güncelle
async function updateStats() {
    try {
        const devices = await fetchAPI(API.devices);
        console.log('Updating stats with devices:', devices);
        
        // Aktif cihazları filtrele
        const activeDevices = devices.filter(d => {
            const isActive = d.Status === 'ON' || d.IsActive === true;
            console.log(`Device ${d.Name}: Status=${d.Status}, IsActive=${d.IsActive}, isActive=${isActive}`);
            return isActive;
        });
        
        const totalDevices = devices.length;
        const activeDeviceCount = activeDevices.length;

        console.log(`Active devices: ${activeDeviceCount}/${totalDevices}`);

        // Aktif cihazları güncelle
        document.querySelector('.stat-card:first-child p').textContent = `${activeDeviceCount}/${totalDevices}`;

        // Gerçekçi sıcaklık hesaplama
        let averageTemp = calculateAverageTemperature(activeDevices);
        
        // Gerçekçi enerji tüketimi hesaplama
        let totalEnergyUsage = calculateEnergyUsage(activeDevices);

        document.querySelector('.stat-card:nth-child(2) p').textContent = `${averageTemp}°C`;
        document.querySelector('.stat-card:last-child p').textContent = `${totalEnergyUsage} kW`;

        console.log(`Stats updated - Active: ${activeDeviceCount}/${totalDevices}, Temp: ${averageTemp}°C, Energy: ${totalEnergyUsage} kW`);

    } catch (error) {
        console.error('Error updating stats:', error);
    }
}

// Ortalama sıcaklık hesaplama
// Sıcaklık hesaplama - Tahmine dayalı akıllı sistem
let lastTemperature = 22.0;
let lastActiveDevices = [];
let temperatureHistory = []; // Son 24 saatin verileri
let deviceUsagePatterns = {}; // Cihaz kullanım kalıpları

function calculateAverageTemperature(activeDevices) {
    const currentTime = new Date();
    const currentHour = currentTime.getHours();
    
    // Geçmiş verileri kaydet
    recordTemperatureHistory(currentTime, lastTemperature, activeDevices);
    
    if (activeDevices.length === 0) {
        // Tahmine dayalı: Bu saatte genelde nasıl?
        const historicalAvg = getHistoricalAverage(currentHour);
        const targetTemp = historicalAvg || 22.0;
        
        const tempDifference = targetTemp - lastTemperature;
        const changeRate = 0.1;
        lastTemperature += tempDifference * changeRate;
        
        lastActiveDevices = [];
        return lastTemperature.toFixed(1);
    }

    // Sıcaklık etkisi olan cihazlar
    const temperatureEffects = {
        'AC': -2.5,         // Klima soğutur
        'THERMOSTAT': 1.2,  // Termostat ısıtır
        'TV': 0.15,         // TV çok hafif ısıtır
        'LIGHT': 0.08,      // LED ışık çok az ısıtır
        'SPEAKER': 0.05,    // Hoparlör çok az ısıtır
        'CAMERA': 0.02,     // Kamera çok az ısıtır
        'LOCK': 0,          // Kilit sıcaklığı etkilemez
        'CURTAIN': 0        // Perde sıcaklığı etkilemez
    };
    
    // 1. Temel hedef sıcaklık (saatlik kalıp)
    const baseTemp = getHourlyPattern(currentHour);
    
    // 2. Cihaz etkilerini tahmin et
    let predictedEffect = 0;
    activeDevices.forEach(device => {
        const effect = temperatureEffects[device.Type] || 0;
        
        // Cihazın geçmiş kullanım kalıbına göre etki hesapla
        const usagePattern = getDeviceUsagePattern(device.Id, currentHour);
        const adjustedEffect = effect * usagePattern.efficiencyFactor;
        
        predictedEffect += adjustedEffect;
    });
    
    const targetTemp = baseTemp + predictedEffect;
    
    // 3. Cihaz durumu değişimini analiz et
    const deviceChange = analyzeDeviceChange(activeDevices, lastActiveDevices);
    
    if (deviceChange.hasChanged) {
        // Akıllı değişim hızı: Geçmiş verilere göre
        const historicalChangeRate = getHistoricalChangeRate(deviceChange.type);
        const tempDifference = targetTemp - lastTemperature;
        const smartChangeRate = Math.min(historicalChangeRate, Math.abs(tempDifference) * 0.3);
        
        const tempChange = tempDifference * smartChangeRate;
        lastTemperature += tempChange;
        
        console.log(`🔮 Tahmine dayalı: ${(lastTemperature - tempChange).toFixed(1)}°C → ${targetTemp.toFixed(1)}°C (akıllı değişim: ${tempChange.toFixed(2)}°C)`);
    } else {
        // Doğal dalgalanma + hedef sıcaklığa yavaş yakınlaşma
        const naturalFluctuation = (Math.random() - 0.5) * 0.08; // ±0.04°C
        const drift = (targetTemp - lastTemperature) * 0.03; // Çok yavaş yakınlaşma
        
        lastTemperature += naturalFluctuation + drift;
    }
    
    // Son cihaz listesini güncelle
    lastActiveDevices = [...activeDevices];
    
    // Sınırla
    lastTemperature = Math.max(18, Math.min(28, lastTemperature));
    
    return lastTemperature.toFixed(1);
}

// Geçmiş veri kaydetme
function recordTemperatureHistory(time, temp, devices) {
    temperatureHistory.push({
        time: time,
        temperature: temp,
        devices: devices.map(d => ({id: d.Id, type: d.Type})),
        hour: time.getHours()
    });
    
    // Son 24 saati tut
    const oneDayAgo = new Date(time.getTime() - 24 * 60 * 60 * 1000);
    temperatureHistory = temperatureHistory.filter(record => record.time > oneDayAgo);
}

// Saatlik kalıp analizi
function getHourlyPattern(hour) {
    const hourlyData = temperatureHistory.filter(record => record.hour === hour);
    
    if (hourlyData.length === 0) {
        // Varsayılan saatlik kalıp
        const defaultPattern = {
            0: 21.5, 1: 21.0, 2: 20.8, 3: 20.5, 4: 20.3, 5: 20.5,
            6: 21.0, 7: 21.5, 8: 22.0, 9: 22.2, 10: 22.5, 11: 22.8,
            12: 23.0, 13: 23.2, 14: 23.5, 15: 23.8, 16: 23.5, 17: 23.0,
            18: 22.5, 19: 22.3, 20: 22.0, 21: 21.8, 22: 21.5, 23: 21.3
        };
        return defaultPattern[hour] || 22.0;
    }
    
    const avgTemp = hourlyData.reduce((sum, record) => sum + record.temperature, 0) / hourlyData.length;
    return avgTemp;
}

// Cihaz kullanım kalıbı analizi
function getDeviceUsagePattern(deviceId, hour) {
    if (!deviceUsagePatterns[deviceId]) {
        deviceUsagePatterns[deviceId] = {
            efficiencyFactor: 1.0,
            hourlyUsage: {},
            totalUsage: 0
        };
    }
    
    const pattern = deviceUsagePatterns[deviceId];
    pattern.totalUsage++;
    pattern.hourlyUsage[hour] = (pattern.hourlyUsage[hour] || 0) + 1;
    
    // Sık kullanılan saatlerde verimlilik azalır (cihaz yorulur)
    const usageFrequency = pattern.hourlyUsage[hour] / pattern.totalUsage;
    pattern.efficiencyFactor = Math.max(0.7, 1.0 - usageFrequency * 0.3);
    
    return pattern;
}

// Cihaz değişim analizi
function analyzeDeviceChange(current, previous) {
    const currentIds = current.map(d => d.Id).sort();
    const previousIds = previous.map(d => d.Id).sort();
    
    const added = currentIds.filter(id => !previousIds.includes(id));
    const removed = previousIds.filter(id => !currentIds.includes(id));
    
    return {
        hasChanged: added.length > 0 || removed.length > 0,
        added: added,
        removed: removed,
        type: added.length > removed.length ? 'heating' : 'cooling'
    };
}

// Geçmiş değişim hızı analizi
function getHistoricalChangeRate(changeType) {
    const relevantHistory = temperatureHistory.slice(-10); // Son 10 kayıt
    
    if (relevantHistory.length < 2) return 0.3; // Varsayılan
    
    let totalChangeRate = 0;
    let changeCount = 0;
    
    for (let i = 1; i < relevantHistory.length; i++) {
        const tempDiff = Math.abs(relevantHistory[i].temperature - relevantHistory[i-1].temperature);
        if (tempDiff > 0.1) { // Anlamlı değişim
            totalChangeRate += Math.min(tempDiff / 2, 0.5); // Maksimum %50
            changeCount++;
        }
    }
    
    return changeCount > 0 ? totalChangeRate / changeCount : 0.3;
}

// Geçmiş ortalama
function getHistoricalAverage(hour) {
    const hourlyData = temperatureHistory.filter(record => 
        record.hour === hour && record.devices.length === 0
    );
    
    if (hourlyData.length === 0) return null;
    
    return hourlyData.reduce((sum, record) => sum + record.temperature, 0) / hourlyData.length;
}

// Enerji tüketimi hesaplama
function calculateEnergyUsage(activeDevices) {
    if (activeDevices.length === 0) {
        return '0.0';
    }
    
    // Cihaz türlerine göre gerçekçi güç tüketimleri (kW)
    const powerConsumption = {
        'AC': 2.5,         // Klima yüksek tüketim
        'TV': 0.15,        // Televizyon orta tüketim
        'THERMOSTAT': 1.8, // Termostat yüksek tüketim
        'LIGHT': 0.06,     // LED ışık düşük tüketim
        'SPEAKER': 0.08,   // Hoparlör düşük tüketim
        'CAMERA': 0.02,    // Kamera çok düşük tüketim
        'LOCK': 0.01,      // Akıllı kilit çok düşük tüketim
        'CURTAIN': 0.05    // Motorlu perde düşük tüketim
    };
    
    let totalPower = 0;
    
    // Her aktif cihazın güç tüketimini topla
    activeDevices.forEach(device => {
        const basePower = powerConsumption[device.Type] || 0.1; // Varsayılan 0.1 kW
        
        // ±20% varyasyon ekle (cihazların farklı modlarda çalışması)
        const variation = 1 + (Math.random() - 0.5) * 0.4;
        const devicePower = basePower * variation;
        
        totalPower += devicePower;
        
        console.log(`Device ${device.Name} (${device.Type}): ${devicePower.toFixed(3)} kW`);
    });
    
    return totalPower.toFixed(1);
}

// Cihaz tipine göre ikon seç
function getDeviceIcon(type) {
    const icons = {
        'LIGHT': 'lightbulb',
        'TV': 'tv',
        'AC': 'fan',
        'THERMOSTAT': 'thermometer-half',
        'SPEAKER': 'volume-up',
        'CAMERA': 'video',
        'LOCK': 'lock',
        'CURTAIN': 'blinds'
    };
    return icons[type] || 'plug';
}

// Senaryo adına göre ikon seç
function getSceneIcon(name) {
    if (!name) return 'magic';
    
    const lowercaseName = name.toLowerCase();
    if (lowercaseName.includes('gece')) return 'moon';
    if (lowercaseName.includes('çıkış')) return 'door-open';
    if (lowercaseName.includes('film')) return 'film';
    if (lowercaseName.includes('parti')) return 'music';
    if (lowercaseName.includes('sabah')) return 'sun';
    return 'magic';
}

// Sayfa navigasyon fonksiyonu
function showPage(pageTitle) {
    // Tüm sayfaları gizle
    const allPages = document.querySelectorAll('.page-content');
    allPages.forEach(page => page.classList.remove('active'));
    
    // İlgili sayfayı göster
    let targetPageId;
    switch(pageTitle) {
        case 'Dashboard':
            targetPageId = 'dashboard-page';
            break;
        case 'Odalar':
            targetPageId = 'rooms-page';
            loadRoomsPage();
            break;
        case 'Senaryolar':
            targetPageId = 'scenes-page';
            loadScenesPage();
            break;
        case 'Ayarlar':
            targetPageId = 'settings-page';
            break;
        default:
            targetPageId = 'dashboard-page';
    }
    
    const targetPage = document.getElementById(targetPageId);
    if (targetPage) {
        targetPage.classList.add('active');
    }
}

// Odalar sayfasını yükle
async function loadRoomsPage() {
    try {
        const rooms = await fetchAPI(API.rooms);
        console.log('Loading rooms page with:', rooms);
        
        const roomsGrid = document.querySelector('.rooms-grid');
        if (!rooms || rooms.length === 0) {
            roomsGrid.innerHTML = '<div class="empty-state">Henüz oda bulunmuyor</div>';
            return;
        }
        
        roomsGrid.innerHTML = rooms.map(room => {
            const roomId = room.Id;
            const roomName = room.Name || 'Adsız Oda';
            const roomDevices = room.Devices || [];
            const deviceCount = roomDevices.length;
            const activeDevices = roomDevices.filter(d => d.Status === 'ON').length;
            
            return `
                <div class="room-detail-card" data-room-id="${roomId}">
                    <div class="room-detail-header">
                        <h3>${roomName}</h3>
                        <div class="room-actions">
                            <button class="edit-btn" onclick="editRoom(${roomId})">
                                <i class="fas fa-edit"></i> Düzenle
                            </button>
                            <button class="delete-btn" onclick="deleteRoom(${roomId})">
                                <i class="fas fa-trash"></i> Sil
                            </button>
                        </div>
                    </div>
                    <div class="room-stats">
                        <p><strong>Toplam Cihaz:</strong> ${deviceCount}</p>
                        <p><strong>Aktif Cihaz:</strong> ${activeDevices}</p>
                        <p><strong>Kat:</strong> ${room.Floor || 1}</p>
                    </div>
                    <div class="room-devices">
                        ${roomDevices.length > 0 ? roomDevices.map(device => {
                            const isActive = device.Status === 'ON';
                            return `
                                <div class="device-item ${isActive ? 'active' : ''}" data-device-id="${device.Id}">
                                    <i class="fas fa-${getDeviceIcon(device.Type)}"></i>
                                    <span>${device.Name}</span>
                                </div>
                            `;
                        }).join('') : '<div class="empty-device">Bu odada cihaz bulunmuyor</div>'}
                    </div>
                </div>
            `;
        }).join('');
    } catch (error) {
        console.error('Error loading rooms page:', error);
    }
}

// Senaryolar sayfasını yükle
async function loadScenesPage() {
    try {
        const scenes = await fetchAPI(API.scenes);
        console.log('Loading scenes page with:', scenes);
        
        const scenesGrid = document.querySelector('.scenes-grid');
        if (!scenes || scenes.length === 0) {
            scenesGrid.innerHTML = '<div class="empty-state">Henüz senaryo bulunmuyor</div>';
            return;
        }
        
        scenesGrid.innerHTML = scenes.map(scene => {
            const sceneId = scene.Id;
            const sceneName = scene.Name || 'Adsız Senaryo';
            const sceneDescription = scene.Description || 'Açıklama yok';
            
            return `
                <div class="scene-detail-card" data-scene-id="${sceneId}">
                    <i class="fas fa-${getSceneIcon(sceneName)}"></i>
                    <h3>${sceneName}</h3>
                    <p>${sceneDescription}</p>
                    <div class="scene-actions">
                        <button class="scene-trigger primary-btn" data-scene-id="${sceneId}">
                            <i class="fas fa-play"></i> Çalıştır
                        </button>
                        <button class="edit-btn" onclick="editScene(${sceneId})">
                            <i class="fas fa-edit"></i> Düzenle
                        </button>
                        <button class="delete-btn" onclick="deleteScene(${sceneId})">
                            <i class="fas fa-trash"></i> Sil
                        </button>
                    </div>
                </div>
            `;
        }).join('');
    } catch (error) {
        console.error('Error loading scenes page:', error);
    }
}

// Cihaz düzenleme fonksiyonu
async function editDevice(deviceId) {
    try {
        // Cihaz bilgilerini getir
        const device = await fetchAPI(`${API.devices}/${deviceId}`);
        
        // Odaları yükle
        await loadRoomsToEditSelect();
        
        // Modal formunu doldur
        document.getElementById('edit-device-id').value = device.Id;
        document.getElementById('edit-device-name').value = device.Name;
        document.getElementById('edit-device-type').value = device.Type;
        document.getElementById('edit-device-room').value = device.RoomId;
        document.getElementById('edit-device-ip').value = device.IpAddress || '';
        document.getElementById('edit-device-mac').value = device.MacAddress || '';
        
        // Modalı aç
        document.getElementById('edit-device-modal').style.display = 'block';
    } catch (error) {
        console.error('Error editing device:', error);
        showNotification('Cihaz düzenlenirken hata oluştu: ' + error.message, 'error');
    }
}

// Cihaz silme fonksiyonu
async function deleteDevice(deviceId) {
    if (confirm('Bu cihazı silmek istediğinizden emin misiniz?')) {
        try {
            await fetchAPI(`${API.devices}/${deviceId}`, {
                method: 'DELETE'
            });
            
            showNotification('Cihaz başarıyla silindi!', 'success');
            loadDashboard(); // Sayfayı yenile
        } catch (error) {
            console.error('Error deleting device:', error);
            showNotification('Cihaz silinirken hata oluştu: ' + error.message, 'error');
        }
    }
}

// Oda düzenleme fonksiyonu
async function editRoom(roomId) {
    try {
        // Oda bilgilerini getir
        const room = await fetchAPI(`${API.rooms}/${roomId}`);
        
        // Modal formunu doldur
        document.getElementById('edit-room-id').value = room.Id;
        document.getElementById('edit-room-name').value = room.Name;
        document.getElementById('edit-room-description').value = room.Description || '';
        document.getElementById('edit-room-floor').value = room.Floor || 1;
        
        // Modalı aç
        document.getElementById('edit-room-modal').style.display = 'block';
    } catch (error) {
        console.error('Error editing room:', error);
        showNotification('Oda düzenlenirken hata oluştu: ' + error.message, 'error');
    }
}

// Oda silme fonksiyonu
async function deleteRoom(roomId) {
    if (confirm('Bu odayı silmek istediğinizden emin misiniz? Odadaki tüm cihazlar da silinecektir.')) {
        try {
            await fetchAPI(`${API.rooms}/${roomId}`, {
                method: 'DELETE'
            });
            
            showNotification('Oda başarıyla silindi!', 'success');
            loadRoomsPage(); // Sayfayı yenile
            loadDashboard(); // Dashboard'u da yenile
        } catch (error) {
            console.error('Error deleting room:', error);
            showNotification('Oda silinirken hata oluştu: ' + error.message, 'error');
        }
    }
}

// Senaryo düzenleme fonksiyonu
async function editScene(sceneId) {
    try {
        // Senaryo bilgilerini getir
        const scene = await fetchAPI(`${API.scenes}/${sceneId}`);
        
        // Modal formunu doldur
        document.getElementById('edit-scene-id').value = scene.Id;
        document.getElementById('edit-scene-name').value = scene.Name;
        document.getElementById('edit-scene-description').value = scene.Description || '';
        document.getElementById('edit-scene-icon').value = scene.Icon || 'magic';
        
        // Modalı aç
        document.getElementById('edit-scene-modal').style.display = 'block';
    } catch (error) {
        console.error('Error editing scene:', error);
        showNotification('Senaryo düzenlenirken hata oluştu: ' + error.message, 'error');
    }
}

// Senaryo silme fonksiyonu
async function deleteScene(sceneId) {
    if (confirm('Bu senaryoyu silmek istediğinizden emin misiniz?')) {
        try {
            await fetchAPI(`${API.scenes}/${sceneId}`, {
                method: 'DELETE'
            });
            
            showNotification('Senaryo başarıyla silindi!', 'success');
            loadScenesPage(); // Sayfayı yenile
            loadDashboard(); // Dashboard'u da yenile
        } catch (error) {
            console.error('Error deleting scene:', error);
            showNotification('Senaryo silinirken hata oluştu: ' + error.message, 'error');
        }
    }
}

// Logout functionality
function setupLogout() {
    // Logout buton click handler'ı zaten HTML'de onclick ile tanımlı
    // Ek setup gerekmiyor
    console.log('Logout functionality ready');
}

function logout() {
    if (confirm('Çıkış yapmak istediğinize emin misiniz?')) {
        try {
            // Clear stored data
            localStorage.removeItem('token');
            localStorage.removeItem('user');
            
            // Show success message
            alert('Başarıyla çıkış yaptınız!');
            
            // Redirect to login page
            window.location.href = '/login.html';
        } catch (error) {
            console.error('Logout error:', error);
            alert('Çıkış yapılırken bir hata oluştu!');
        }
    }
}

// Settings functionality
function setupSettings() {
    // Load saved settings from localStorage
    loadSettings();
    
    // Setup event listeners for settings
    setupSettingsEventListeners();
    
    // Setup auto logout listeners
    setupAutoLogoutListeners();
    
    // Update system info immediately
    updateSystemInfo();
    
    // Update system info every 30 seconds
    setInterval(updateSystemInfo, 30000);
    
    // Update connection info every 10 seconds
    setInterval(updateConnectionInfo, 10000);
}

function loadSettings() {
    const settings = JSON.parse(localStorage.getItem('appSettings') || '{}');
    
    // Apply saved settings
    Object.keys(settings).forEach(key => {
        const element = document.getElementById(key);
        if (element) {
            if (element.type === 'checkbox') {
                element.checked = settings[key];
            } else if (element.tagName === 'SELECT') {
                element.value = settings[key];
            }
        }
    });
    
    // Apply settings immediately
    if (settings['theme-color']) {
        applyThemeColor(settings['theme-color']);
    }
    if (settings['language-select']) {

    }
    if (settings['animations'] !== undefined) {
        applyAnimations(settings['animations']);
    }
    if (settings['compact-view'] !== undefined) {
        applyCompactView(settings['compact-view']);
    }
    if (settings['auto-logout'] !== undefined) {
        applyAutoLogout(settings['auto-logout']);
    }
}

function saveSettings() {
    const settings = {};
    
    // Collect all setting values
    const settingElements = document.querySelectorAll('.setting-item input, .setting-item select');
    settingElements.forEach(element => {
        if (element.id) {
            if (element.type === 'checkbox') {
                settings[element.id] = element.checked;
            } else {
                settings[element.id] = element.value;
            }
        }
    });
    
    localStorage.setItem('appSettings', JSON.stringify(settings));
    
    // Show feedback
    showNotification('Ayarlar kaydedildi!', 'success');
}

function setupSettingsEventListeners() {
    // Auto-save settings on change
    document.addEventListener('change', function(e) {
        if (e.target.closest('.setting-item')) {
            saveSettings();
            
            // Apply specific setting changes immediately
            if (e.target.id === 'theme-color') {
                applyThemeColor(e.target.value);

            } else if (e.target.id === 'animations') {
                applyAnimations(e.target.checked);
            } else if (e.target.id === 'compact-view') {
                applyCompactView(e.target.checked);
            } else if (e.target.id === 'auto-logout') {
                applyAutoLogout(e.target.value);
            }
        }
    });
    
    // Button event listeners
    document.getElementById('change-password-btn')?.addEventListener('click', function() {
        showChangePasswordModal();
    });
    
    document.getElementById('export-data-btn')?.addEventListener('click', function() {
        exportData();
    });
    
    document.getElementById('clear-cache-btn')?.addEventListener('click', function() {
        clearCache();
    });
    
    document.getElementById('reset-settings-btn')?.addEventListener('click', function() {
        resetSettings();
    });
    
    document.getElementById('logout-btn')?.addEventListener('click', function() {
        logout();
    });
}

function applyThemeColor(color) {
    const root = document.documentElement;
    const colors = {
        blue: '#2196F3',
        green: '#4CAF50',
        purple: '#9C27B0',
        orange: '#FF9800'
    };
    
    if (colors[color]) {
        root.style.setProperty('--primary-color', colors[color]);
        showNotification(`Tema rengi ${color} olarak değiştirildi`, 'success');
    }
}



function applyAnimations(enabled) {
    if (enabled) {
        document.body.classList.remove('no-animations');
    } else {
        document.body.classList.add('no-animations');
    }
    showNotification(`Animasyonlar ${enabled ? 'etkinleştirildi' : 'devre dışı bırakıldı'}`, 'success');
}

function applyCompactView(enabled) {
    if (enabled) {
        document.body.classList.add('compact-view');
        showNotification('Kompakt görünüm etkinleştirildi', 'success');
    } else {
        document.body.classList.remove('compact-view');
        showNotification('Normal görünüm etkinleştirildi', 'success');
    }
}

// Otomatik çıkış değişkenleri
let autoLogoutTimer = null;
let autoLogoutWarningTimer = null;
let autoLogoutMinutes = 30; // Varsayılan 30 dakika

function applyAutoLogout(minutes) {
    autoLogoutMinutes = parseInt(minutes);
    
    // Mevcut timer'ları temizle
    if (autoLogoutTimer) {
        clearTimeout(autoLogoutTimer);
        autoLogoutTimer = null;
    }
    if (autoLogoutWarningTimer) {
        clearTimeout(autoLogoutWarningTimer);
        autoLogoutWarningTimer = null;
    }
    
    if (autoLogoutMinutes > 0) {
        startAutoLogoutTimer();
        showNotification(`Otomatik çıkış ${autoLogoutMinutes} dakika olarak ayarlandı (Popup'a 1 dk içinde yanıt verin)`, 'success');
    } else {
        showNotification('Otomatik çıkış devre dışı bırakıldı', 'info');
    }
}

function startAutoLogoutTimer() {
    if (autoLogoutMinutes <= 0) return;
    
    const logoutTime = autoLogoutMinutes * 60 * 1000; // Milisaniye
    const warningTime = Math.max(logoutTime - (60 * 1000), logoutTime - 60000); // 1 dakika önce uyar
    
    console.log(`Auto logout timer started: ${autoLogoutMinutes} minutes, warning at ${warningTime}ms, logout at ${logoutTime}ms`);
    
    // Sadece uyarı timer'ı - ana timer'ı kaldırdık
    autoLogoutWarningTimer = setTimeout(() => {
        const remainingMinutes = 1; // Her zaman 1 dakika kalıyor
        
        // Bildirim göster
        showNotification(`${remainingMinutes} dakika sonra otomatik çıkış yapılacak. Herhangi bir aktivite yapın.`, 'warning');
        
        // 1 dakika sonra otomatik çıkış yap
        autoLogoutTimer = setTimeout(() => {
            console.log('Auto logout after warning - no user activity detected');
            showNotification('Otomatik çıkış yapılıyor...', 'error');
            setTimeout(() => {
                logout();
            }, 1000);
        }, 60000); // 1 dakika
        
    }, warningTime);
}

function resetAutoLogoutTimer() {
    // Timer'ları temizle ve yeniden başlat
    console.log('Resetting auto logout timer due to user activity');
    
    const wasWarningActive = autoLogoutWarningTimer !== null;
    const wasLogoutActive = autoLogoutTimer !== null;
    
    if (autoLogoutTimer) {
        clearTimeout(autoLogoutTimer);
        autoLogoutTimer = null;
    }
    if (autoLogoutWarningTimer) {
        clearTimeout(autoLogoutWarningTimer);
        autoLogoutWarningTimer = null;
    }
    
    // Eğer uyarı sonrası aktivite varsa bildirim göster
    if (wasLogoutActive) {
        showNotification('Oturum uzatıldı', 'success');
    }
    
    // Sadece otomatik çıkış etkinse yeniden başlat
    if (autoLogoutMinutes > 0) {
        startAutoLogoutTimer();
    }
}

// Kullanıcı aktivitesi dinleyicileri
function setupAutoLogoutListeners() {
    let lastResetTime = 0;
    const throttleTime = 5000; // 5 saniyede bir reset
    
    const throttledReset = () => {
        const now = Date.now();
        if (now - lastResetTime > throttleTime) {
            lastResetTime = now;
            resetAutoLogoutTimer();
        }
    };
    
    const events = ['mousedown', 'keypress', 'click', 'touchstart'];
    
    events.forEach(event => {
        document.addEventListener(event, throttledReset, true);
    });
}

function showChangePasswordModal() {
    const modal = document.getElementById('change-password-modal');
    modal.style.display = 'block';
    
    // Clear form
    document.getElementById('change-password-form').reset();
}

async function changePassword(formData) {
    try {
        console.log('Sending change password request:', formData);
        console.log('API URL:', `${API.user}/change-password`);
        
        const response = await fetchAPI(`${API.user}/change-password`, {
            method: 'POST',
            body: JSON.stringify(formData)
        });
        
        showNotification('Şifre başarıyla değiştirildi', 'success');
        
        // Close modal
        const modal = document.getElementById('change-password-modal');
        modal.style.display = 'none';
        
        // Clear form
        document.getElementById('change-password-form').reset();
        
        return true;
    } catch (error) {
        console.error('Error changing password:', error);
        showNotification('Şifre değiştirilemedi: ' + error.message, 'error');
        return false;
    }
}

function exportData() {
    try {
        const data = {
            settings: JSON.parse(localStorage.getItem('appSettings') || '{}'),
            user: JSON.parse(localStorage.getItem('user') || '{}'),
            exportDate: new Date().toISOString()
        };
        
        const blob = new Blob([JSON.stringify(data, null, 2)], { type: 'application/json' });
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `smarthome-data-${new Date().toISOString().split('T')[0]}.json`;
        a.click();
        URL.revokeObjectURL(url);
        
        showNotification('Veriler başarıyla dışa aktarıldı', 'success');
    } catch (error) {
        showNotification('Veri dışa aktarma hatası: ' + error.message, 'error');
    }
}

function clearCache() {
    if (confirm('Önbellek temizlenecek. Devam etmek istiyor musunuz?')) {
        // Clear various caches
        if ('caches' in window) {
            caches.keys().then(names => {
                names.forEach(name => {
                    caches.delete(name);
                });
            });
        }
        
        // Clear localStorage except essential items
        const essentialKeys = ['token', 'user'];
        const keysToRemove = [];
        for (let i = 0; i < localStorage.length; i++) {
            const key = localStorage.key(i);
            if (!essentialKeys.includes(key)) {
                keysToRemove.push(key);
            }
        }
        keysToRemove.forEach(key => localStorage.removeItem(key));
        
        showNotification('Önbellek temizlendi', 'success');
        setTimeout(() => window.location.reload(), 1000);
    }
}

function resetSettings() {
    if (confirm('Tüm ayarlar sıfırlanacak. Bu işlem geri alınamaz!')) {
        localStorage.removeItem('appSettings');
        showNotification('Ayarlar sıfırlandı', 'success');
        setTimeout(() => window.location.reload(), 1000);
    }
}

async function updateConnectionInfo() {
    try {
        console.log('Updating connection information...');
        
        // Get network connection info
        const connection = navigator.connection || navigator.mozConnection || navigator.webkitConnection;
        
        // Update WiFi network name
        const wifiNetworkElement = document.getElementById('wifi-network');
        if (wifiNetworkElement) {
            if (connection && connection.type) {
                switch(connection.type) {
                    case 'wifi':
                        wifiNetworkElement.textContent = 'WiFi Bağlantısı';
                        break;
                    case 'ethernet':
                        wifiNetworkElement.textContent = 'Ethernet Bağlantısı';
                        break;
                    case 'cellular':
                        wifiNetworkElement.textContent = 'Mobil Veri';
                        break;
                    default:
                        wifiNetworkElement.textContent = 'Bilinmeyen Ağ';
                }
            } else {
                // Try to detect by checking if we're on localhost or LAN
                const hostname = window.location.hostname;
                if (hostname === 'localhost' || hostname === '127.0.0.1') {
                    wifiNetworkElement.textContent = 'Yerel Sunucu';
                } else if (hostname.startsWith('192.168.') || hostname.startsWith('10.') || hostname.startsWith('172.')) {
                    wifiNetworkElement.textContent = 'Yerel Ağ (LAN)';
                } else {
                    wifiNetworkElement.textContent = 'İnternet Bağlantısı';
                }
            }
        }
        
        // Update signal strength based on connection quality
        const signalElement = document.querySelector('.signal-strength span');
        if (signalElement) {
            // Test connection speed/quality by measuring API response time
            const startTime = performance.now();
            try {
                await fetch(API.devices, { 
                    method: 'GET',
                    headers: { 'Authorization': `Bearer ${localStorage.getItem('token')}` }
                });
                const responseTime = performance.now() - startTime;
                
                let signalStrength = '';
                let signalClass = '';
                
                if (responseTime < 100) {
                    signalStrength = 'Mükemmel (95%)';
                    signalClass = 'signal-excellent';
                } else if (responseTime < 300) {
                    signalStrength = 'Güçlü (85%)';
                    signalClass = 'signal-good';
                } else if (responseTime < 1000) {
                    signalStrength = 'Orta (65%)';
                    signalClass = 'signal-medium';
                } else {
                    signalStrength = 'Zayıf (35%)';
                    signalClass = 'signal-weak';
                }
                
                signalElement.textContent = signalStrength;
                signalElement.className = signalClass;
                
                // Update signal icon based on strength
                const signalIcon = document.querySelector('.signal-strength i');
                if (signalIcon) {
                    if (responseTime < 300) {
                        signalIcon.className = 'fas fa-signal';
                    } else if (responseTime < 1000) {
                        signalIcon.className = 'fas fa-signal-3';
                    } else {
                        signalIcon.className = 'fas fa-signal-1';
                    }
                }
                
            } catch (error) {
                signalElement.textContent = 'Bağlantı Yok (0%)';
                signalElement.className = 'signal-none';
                
                const signalIcon = document.querySelector('.signal-strength i');
                if (signalIcon) {
                    signalIcon.className = 'fas fa-signal-slash';
                }
            }
        }
        
        // Update auto-reconnect status based on online/offline events
        const autoReconnectElement = document.getElementById('auto-reconnect');
        if (autoReconnectElement) {
            // Set based on browser online status
            autoReconnectElement.checked = navigator.onLine;
            
            // Add event listeners for online/offline events
            if (!window.connectionListenersAdded) {
                window.addEventListener('online', () => {
                    if (autoReconnectElement) {
                        autoReconnectElement.checked = true;
                        showNotification('İnternet bağlantısı yeniden kuruldu', 'success');
                        updateConnectionInfo(); // Refresh connection info
                    }
                });
                
                window.addEventListener('offline', () => {
                    if (autoReconnectElement) {
                        autoReconnectElement.checked = false;
                        showNotification('İnternet bağlantısı kesildi', 'warning');
                        updateConnectionInfo(); // Refresh connection info
                    }
                });
                
                window.connectionListenersAdded = true;
            }
        }
        
        console.log('Connection information updated successfully');
        
    } catch (error) {
        console.error('Error updating connection info:', error);
        
        // Set error values
        const wifiNetworkElement = document.getElementById('wifi-network');
        if (wifiNetworkElement) {
            wifiNetworkElement.textContent = 'Bağlantı Hatası';
        }
        
        const signalElement = document.querySelector('.signal-strength span');
        if (signalElement) {
            signalElement.textContent = 'Hata (0%)';
            signalElement.className = 'signal-error';
        }
    }
}

async function updateSystemInfo() {
    try {
        console.log('Updating system information...');
        
        // Set session start time if not exists
        if (!localStorage.getItem('sessionStartTime')) {
            localStorage.setItem('sessionStartTime', Date.now().toString());
        }
        
        // Update connection info
        await updateConnectionInfo();
        
        // Update API version - just set it directly without unnecessary API call
        const apiVersionElement = document.getElementById('api-version');
        if (apiVersionElement) {
            apiVersionElement.textContent = 'v1.2.0';
        }
        
        // Update database status
        const dbStatusElement = document.getElementById('db-status');
        try {
            await fetchAPI(API.devices);
            if (dbStatusElement) {
                dbStatusElement.className = 'status-online';
                dbStatusElement.innerHTML = '<i class="fas fa-circle"></i> Çevrimiçi';
            }
        } catch (error) {
            if (dbStatusElement) {
                dbStatusElement.className = 'status-error';
                dbStatusElement.innerHTML = '<i class="fas fa-times-circle"></i> Hata';
            }
        }
        
        // Update system status
        const systemStatusElement = document.getElementById('system-status');
        const errorCount = document.querySelectorAll('.status-error').length;
        if (systemStatusElement) {
            if (errorCount === 0) {
                systemStatusElement.className = 'status-online';
                systemStatusElement.innerHTML = '<i class="fas fa-circle"></i> Normal';
            } else if (errorCount <= 2) {
                systemStatusElement.className = 'status-warning';
                systemStatusElement.innerHTML = '<i class="fas fa-exclamation-triangle"></i> Uyarı';
            } else {
                systemStatusElement.className = 'status-error';
                systemStatusElement.innerHTML = '<i class="fas fa-times-circle"></i> Hata';
            }
        }
        
        // Update session start time
        const sessionStartElement = document.getElementById('session-start');
        if (sessionStartElement) {
            const sessionStart = parseInt(localStorage.getItem('sessionStartTime'));
            const startDate = new Date(sessionStart);
            sessionStartElement.textContent = startDate.toLocaleString('tr-TR', {
                year: 'numeric',
                month: '2-digit',
                day: '2-digit',
                hour: '2-digit',
                minute: '2-digit'
            });
        }
        
        // Update uptime
        const uptimeElement = document.getElementById('uptime');
        if (uptimeElement) {
            const sessionStart = parseInt(localStorage.getItem('sessionStartTime'));
            const uptime = Date.now() - sessionStart;
            const days = Math.floor(uptime / (1000 * 60 * 60 * 24));
            const hours = Math.floor((uptime % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));
            const minutes = Math.floor((uptime % (1000 * 60 * 60)) / (1000 * 60));
            
            if (days > 0) {
                uptimeElement.textContent = `${days} gün ${hours} saat`;
            } else if (hours > 0) {
                uptimeElement.textContent = `${hours} saat ${minutes} dakika`;
            } else {
                uptimeElement.textContent = `${minutes} dakika`;
            }
        }
        

        
        // Update client IP (get from an external service)
        const clientIpElement = document.getElementById('client-ip');
        if (clientIpElement && clientIpElement.textContent === '-') {
            try {
                const ipResponse = await fetch('https://api.ipify.org?format=json');
                const ipData = await ipResponse.json();
                clientIpElement.textContent = ipData.ip || 'Bilinmiyor';
            } catch (e) {
                clientIpElement.textContent = 'Yerel Ağ';
            }
        }
        
        console.log('System information updated successfully');
        
    } catch (error) {
        console.error('Error updating system info:', error);
        
        // Set error status for system
        const systemStatusElement = document.getElementById('system-status');
        if (systemStatusElement) {
            systemStatusElement.className = 'status-error';
            systemStatusElement.innerHTML = '<i class="fas fa-times-circle"></i> Kritik Hata';
        }
    }
}

function showNotification(message, type = 'info') {
    // Create notification element
    const notification = document.createElement('div');
    notification.className = `notification notification-${type}`;
    notification.textContent = message;
    
    // Style the notification
    notification.style.cssText = `
        position: fixed;
        top: 20px;
        right: 20px;
        padding: 15px 20px;
        border-radius: 8px;
        color: white;
        font-weight: 500;
        z-index: 10000;
        animation: slideIn 0.3s ease;
        max-width: 300px;
        box-shadow: 0 4px 12px rgba(0,0,0,0.15);
    `;
    
    // Set background color based on type
    const colors = {
        success: '#4CAF50',
        error: '#F44336',
        info: '#2196F3',
        warning: '#FF9800'
    };
    notification.style.backgroundColor = colors[type] || colors.info;
    
    // Add to page
    document.body.appendChild(notification);
    
    // Remove after 3 seconds
    setTimeout(() => {
        notification.style.animation = 'slideOut 0.3s ease';
        setTimeout(() => {
            if (notification.parentNode) {
                notification.parentNode.removeChild(notification);
            }
        }, 300);
    }, 3000);
}

// Add CSS for notifications
const notificationCSS = `
@keyframes slideIn {
    from { transform: translateX(100%); opacity: 0; }
    to { transform: translateX(0); opacity: 1; }
}
@keyframes slideOut {
    from { transform: translateX(0); opacity: 1; }
    to { transform: translateX(100%); opacity: 0; }
}
.no-animations * {
    animation: none !important;
    transition: none !important;
}
`;

// Add notification styles to head
const style = document.createElement('style');
style.textContent = notificationCSS;
document.head.appendChild(style);

// Set app start time for uptime calculation
if (!localStorage.getItem('appStartTime')) {
    localStorage.setItem('appStartTime', Date.now().toString());
}

// Initialize settings when page loads
document.addEventListener('DOMContentLoaded', function() {
    setupSettings();
});

// Her 30 saniyede bir istatistikleri güncelle
setInterval(updateStats, 30000); 