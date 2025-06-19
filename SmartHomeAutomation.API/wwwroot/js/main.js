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
        const response = await fetch(endpoint, {
            ...options,
            headers: {
                'Content-Type': 'application/json',
                ...options.headers
            }
        });

        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        return await response.json();
    } catch (error) {
        console.error('API Error:', error);
        throw error;
    }
}

document.addEventListener('DOMContentLoaded', async function() {
    // Sayfa yüklendiğinde verileri getir
    try {
        await loadDashboard();
    } catch (error) {
        console.error('Error loading dashboard:', error);
    }

    // Device toggle functionality
    document.addEventListener('click', async function(e) {
        if (e.target.closest('.device-item')) {
            const deviceItem = e.target.closest('.device-item');
            const deviceId = deviceItem.dataset.deviceId;
            const currentState = deviceItem.classList.contains('active');

            try {
                await fetchAPI(`${API.devices}/${deviceId}/toggle`, {
                    method: 'POST',
                    body: JSON.stringify({ state: !currentState })
                });

                deviceItem.classList.toggle('active');
                const deviceName = deviceItem.querySelector('span').textContent;
                const isActive = deviceItem.classList.contains('active');
                console.log(`${deviceName} is now ${isActive ? 'ON' : 'OFF'}`);
            } catch (error) {
                console.error('Error toggling device:', error);
            }
        }
    });

    // Scene trigger functionality
    document.addEventListener('click', async function(e) {
        if (e.target.classList.contains('scene-trigger')) {
            const trigger = e.target;
            const sceneId = trigger.dataset.sceneId;
            const sceneName = trigger.parentElement.querySelector('h3').textContent;

            // Visual feedback
            const originalText = trigger.textContent;
            trigger.textContent = 'Çalıştırılıyor...';
            trigger.disabled = true;

            try {
                await fetchAPI(`${API.scenes}/${sceneId}/execute`, {
                    method: 'POST'
                });
                console.log(`Executed scene: ${sceneName}`);
            } catch (error) {
                console.error('Error executing scene:', error);
            } finally {
                setTimeout(() => {
                    trigger.textContent = originalText;
                    trigger.disabled = false;
                }, 2000);
            }
        }
    });

    // Navigation highlight
    const navLinks = document.querySelectorAll('.nav-links a');
    navLinks.forEach(link => {
        link.addEventListener('click', function(e) {
            e.preventDefault();
            navLinks.forEach(l => l.parentElement.classList.remove('active'));
            this.parentElement.classList.add('active');
            
            const headerTitle = document.querySelector('header h1');
            headerTitle.textContent = this.querySelector('span').textContent;
        });
    });

    // Modal functionality
    setupModals();
    
    // Setup form submissions
    setupFormSubmissions();
    
    // Setup refresh button
    document.querySelector('.refresh-btn').addEventListener('click', loadDashboard);
});

// Setup modal functionality
function setupModals() {
    const addRoomBtn = document.getElementById('add-room-btn');
    const addSceneBtn = document.getElementById('add-scene-btn');
    const roomModal = document.getElementById('add-room-modal');
    const sceneModal = document.getElementById('add-scene-modal');
    const closeButtons = document.querySelectorAll('.close, .cancel-btn');
    
    // Open room modal
    addRoomBtn.addEventListener('click', () => {
        roomModal.style.display = 'block';
    });
    
    // Open scene modal
    addSceneBtn.addEventListener('click', () => {
        sceneModal.style.display = 'block';
    });
    
    // Close modals
    closeButtons.forEach(btn => {
        btn.addEventListener('click', () => {
            roomModal.style.display = 'none';
            sceneModal.style.display = 'none';
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
    });
}

// Setup form submissions
function setupFormSubmissions() {
    const addRoomForm = document.getElementById('add-room-form');
    const addSceneForm = document.getElementById('add-scene-form');
    
    // Room form submission
    addRoomForm.addEventListener('submit', async (e) => {
        e.preventDefault();
        
        const formData = {
            name: document.getElementById('room-name').value,
            description: document.getElementById('room-description').value,
            floor: parseInt(document.getElementById('room-floor').value, 10)
        };
        
        try {
            await fetchAPI(API.rooms, {
                method: 'POST',
                body: JSON.stringify(formData)
            });
            
            // Close modal and reset form
            document.getElementById('add-room-modal').style.display = 'none';
            addRoomForm.reset();
            
            // Refresh rooms
            await loadDashboard();
            
            // Show success message
            alert('Oda başarıyla eklendi!');
        } catch (error) {
            console.error('Error adding room:', error);
            alert('Oda eklenirken bir hata oluştu!');
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
}

// Dashboard verilerini yükle
async function loadDashboard() {
    try {
        // Odaları yükle
        const rooms = await fetchAPI(API.rooms);
        updateRoomsOverview(rooms);

        // Senaryoları yükle
        const scenes = await fetchAPI(API.scenes);
        updateScenesOverview(scenes);

        // İstatistikleri güncelle
        updateStats();
    } catch (error) {
        console.error('Error loading dashboard data:', error);
    }
}

// Oda kartlarını güncelle
function updateRoomsOverview(rooms) {
    const roomCardsContainer = document.querySelector('.room-cards');
    if (!rooms || rooms.length === 0) {
        roomCardsContainer.innerHTML = '<div class="empty-state">Henüz oda bulunmuyor</div>';
        return;
    }
    
    roomCardsContainer.innerHTML = rooms.map(room => `
        <div class="room-card">
            <div class="room-header">
                <h3>${room.name}</h3>
                <span class="device-count">${room.devices ? room.devices.length : 0} Cihaz</span>
            </div>
            <div class="room-devices">
                ${room.devices && room.devices.length > 0 ? room.devices.map(device => `
                    <div class="device-item ${device.isActive ? 'active' : ''}" data-device-id="${device.id}">
                        <i class="fas fa-${getDeviceIcon(device.type)}"></i>
                        <span>${device.name}</span>
                    </div>
                `).join('') : '<div class="empty-device">Bu odada cihaz bulunmuyor</div>'}
            </div>
        </div>
    `).join('');
}

// Senaryo kartlarını güncelle
function updateScenesOverview(scenes) {
    const sceneCardsContainer = document.querySelector('.scene-cards');
    if (!scenes || scenes.length === 0) {
        sceneCardsContainer.innerHTML = '<div class="empty-state">Henüz senaryo bulunmuyor</div>';
        return;
    }
    
    sceneCardsContainer.innerHTML = scenes.map(scene => `
        <div class="scene-card">
            <i class="fas fa-${getSceneIcon(scene.name)}"></i>
            <h3>${scene.name}</h3>
            <button class="scene-trigger" data-scene-id="${scene.id}">Çalıştır</button>
        </div>
    `).join('');
}

// İstatistikleri güncelle
async function updateStats() {
    try {
        const devices = await fetchAPI(API.devices);
        const activeDevices = devices.filter(d => d.isActive).length;
        const totalDevices = devices.length;

        // Aktif cihazları güncelle
        document.querySelector('.stat-card:first-child p').textContent = `${activeDevices}/${totalDevices}`;

        // Sıcaklık ve enerji verilerini al (mock data)
        const temp = (Math.random() * 5 + 20).toFixed(1);
        const energyUsage = (Math.random() * 2 + 1.5).toFixed(1);

        document.querySelector('.stat-card:nth-child(2) p').textContent = `${temp}°C`;
        document.querySelector('.stat-card:last-child p').textContent = `${energyUsage} kW`;

    } catch (error) {
        console.error('Error updating stats:', error);
    }
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

// Her 30 saniyede bir istatistikleri güncelle
setInterval(updateStats, 30000); 