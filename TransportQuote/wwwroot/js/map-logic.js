document.addEventListener('DOMContentLoaded', function () {
    // Initialize the map centered on Cancun
    var map = L.map('map').setView([21.1619, -86.8515], 10);

    // Add OpenStreetMap tile layer
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
    }).addTo(map);

    // Category-based icon configuration using Material Symbols
    const categoryStyles = {
        "Playa": { className: "marker-beach", icon: "beach_access" },
        "Hotel": { className: "marker-hotel", icon: "hotel" },
        "Atracción": { className: "marker-attraction", icon: "attractions" },
        "Aeropuerto": { className: "marker-airport", icon: "flight" },
        "default": { className: "marker-default", icon: "location_on" }
    };
    const categoryImageFallback = {
        "Playa": "https://images.unsplash.com/photo-1507525428034-b723cf961d3e?auto=format&fit=crop&w=900&q=80",
        "Hotel": "https://images.unsplash.com/photo-1542314831-068cd1dbfeeb?auto=format&fit=crop&w=900&q=80",
        "Atracción": "https://images.unsplash.com/photo-1500530855697-b586d89ba3ee?auto=format&fit=crop&w=900&q=80",
        "Aeropuerto": "https://images.unsplash.com/photo-1504198458649-3128b932f49b?auto=format&fit=crop&w=900&q=80",
        "default": "https://images.unsplash.com/photo-1507525428034-b723cf961d3e?auto=format&fit=crop&w=900&q=80"
    };
    const markers = [];
    const locationLookup = {};
    let selectedMarker = null;
    let selectedFromId = null;
    let selectedToId = null;
    const routeLayers = [];

    function getCategoryIcon(type) {
        const style = categoryStyles[type] || categoryStyles["default"];
        return L.divIcon({
            html: `
                <div class="map-marker ${style.className}">
                    <div class="marker-glow"></div>
                    <div class="marker-ring">
                        <div class="marker-core">
                            <span class="material-symbols-rounded">${style.icon}</span>
                        </div>
                    </div>
                </div>
            `,
            className: "",
            iconSize: [60, 60],
            iconAnchor: [30, 54],
            popupAnchor: [0, -52]
        });
    }

    function getSelectionIcon() {
        return L.divIcon({
            html: `
                <div class="selection-pin"></div>
            `,
            className: "",
            iconSize: [46, 46],
            iconAnchor: [23, 23],
            popupAnchor: [0, -40]
        });
    }

    function focusLocationById(id) {
        if (!id || !locationLookup[id]) return;
        const loc = locationLookup[id];

        if (selectedMarker) {
            map.removeLayer(selectedMarker);
        }

        selectedMarker = L.marker([loc.latitude, loc.longitude], {
            icon: getSelectionIcon(),
            zIndexOffset: 1000
        }).addTo(map);

        map.flyTo([loc.latitude, loc.longitude], Math.max(map.getZoom(), 12), { duration: 0.6 });
    }

    function clearRoute() {
        while (routeLayers.length) {
            const layer = routeLayers.pop();
            map.removeLayer(layer);
        }
    }

    function drawRouteIfReady() {
        if (!selectedFromId || !selectedToId || selectedFromId === selectedToId) {
            clearRoute();
            return;
        }

        const from = locationLookup[selectedFromId];
        const to = locationLookup[selectedToId];
        if (!from || !to) return;

        clearRoute();
        const path = [
            [from.latitude, from.longitude],
            [to.latitude, to.longitude]
        ];

        const glow = L.polyline(path, {
            color: 'rgba(99, 190, 255, 0.45)',
            weight: 10,
            opacity: 0.7,
            lineCap: 'round'
        }).addTo(map);

        const core = L.polyline(path, {
            color: '#0a84ff',
            weight: 5,
            opacity: 0.9,
            lineCap: 'round'
        }).addTo(map);

        routeLayers.push(glow, core);
        map.fitBounds([path[0], path[1]], { padding: [80, 80] });
    }

    function applyMarkerScale() {
        const zoom = map.getZoom();
        let scale = 1;
        if (zoom <= 7) {
            scale = 0.55;
        } else if (zoom <= 9) {
            scale = 0.7;
        } else if (zoom <= 11) {
            scale = 0.85;
        } else if (zoom >= 14) {
            scale = 1.05;
        } else {
            scale = 1;
        }

        markers.forEach(function (marker) {
            const el = marker.getElement();
            if (el) {
                el.style.setProperty('--marker-scale', scale);
            }
        });
    }

    // Function to add markers from data
    window.initMapMarkers = function (locations) {
        locations.forEach(function (loc) {
            locationLookup[loc.id] = loc;
            var marker = L.marker([loc.latitude, loc.longitude], { icon: getCategoryIcon(loc.type) }).addTo(map);
            markers.push(marker);

            var imageUrl = loc.imageUrl;
            if (!imageUrl || imageUrl.includes('example.com')) {
                imageUrl = categoryImageFallback[loc.type] || categoryImageFallback["default"];
            }
            
            var popupContent = `
                <div class="popup-content">
                    <div class="popup-cover" style="background-image: url('${imageUrl}');">
                        <div class="popup-cover-overlay"></div>
                    </div>
                    <div class="popup-info">
                        <span class="popup-type">${loc.type}</span>
                        <h3>${loc.name}</h3>
                        <p>${loc.description}</p>
                    </div>
                </div>
            `;

            marker.bindPopup(popupContent);
            marker.on('add', applyMarkerScale);
        });
        applyMarkerScale();

        var fromSelect = document.getElementById('fromLocation');
        if (fromSelect) {
            fromSelect.addEventListener('change', function () {
                selectedFromId = this.value;
                focusLocationById(selectedFromId);
                drawRouteIfReady();
            });
        }

        var toSelect = document.getElementById('toLocation');
        if (toSelect) {
            toSelect.addEventListener('change', function () {
                selectedToId = this.value;
                drawRouteIfReady();
            });
        }
    };

    map.on('zoomend', applyMarkerScale);
});
