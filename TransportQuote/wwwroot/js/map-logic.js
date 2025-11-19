document.addEventListener('DOMContentLoaded', function () {
    var map = L.map('map').setView([21.1619, -86.8515], 10);
    map.createPane('routePane');
    map.getPane('routePane').style.zIndex = 700;

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
    }).addTo(map);

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
    let distanceControl = null;
    const distancePlaceholder = `
        <div class="metric-title">Resumen del trayecto</div>
        <div class="metric-placeholder">Selecciona origen y destino para ver la distancia.</div>
    `;
    const routeList = document.getElementById('routeList');
    const addStopSelect = document.getElementById('addStopSelect');
    const locationOptionsTemplate = document.getElementById('route-location-options');
    const locationOptionsHtml = locationOptionsTemplate ? locationOptionsTemplate.innerHTML : '';
    let draggedStop = null;

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

    function getStopIds() {
        if (!routeList) return [];
        return Array.from(routeList.querySelectorAll('.route-stop--stop select'))
            .map(select => select.value)
            .filter(Boolean);
    }

    function drawRouteIfReady() {
        if (!selectedFromId || !selectedToId || selectedFromId === selectedToId) {
            clearRoute();
            if (distanceControl) {
                distanceControl.innerHTML = distancePlaceholder;
            }
            return;
        }

        const stopIds = getStopIds();

        fetch('/api/routes', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ fromId: selectedFromId, toId: selectedToId, stops: stopIds })
        })
            .then(response => response.ok ? response.json() : null)
            .then(data => {
                if (!data || !data.success || !data.segments || !data.segments.length) {
                    drawFallbackRoute();
                    return;
                }

                renderRouteSegments(data.segments);
                updateDistanceIndicator(data.distanceKm, data.durationMinutes);
            })
            .catch(() => {
                drawFallbackRoute();
            });
    }

    function renderRouteSegments(segments) {
        clearRoute();
        const bounds = [];

        segments.forEach(segment => {
            const coords = segment.coordinates.map(pair => [pair[0], pair[1]]);
            if (!coords.length) return;
            bounds.push(...coords);

            const style = segment.mode === 'sea'
                ? {
                    color: '#18d4ff',
                    weight: 4,
                    opacity: 0.85,
                    dashArray: '10 8',
                    pane: 'routePane'
                }
                : {
                    color: '#0a84ff',
                    weight: 6,
                    opacity: 0.95,
                    pane: 'routePane'
                };

            const line = L.polyline(coords, style).addTo(map);
            routeLayers.push(line);
        });

        if (bounds.length) {
            map.fitBounds(bounds, { padding: [80, 80] });
        }
    }

    function updateDistanceIndicator(distanceKm, durationMinutes) {
        if (!distanceControl) return;
        const hours = Math.floor(durationMinutes / 60);
        const minutes = Math.round(durationMinutes % 60);
        const timeString = hours > 0 ? `${hours} h ${minutes} min` : `${minutes} min`;

        distanceControl.innerHTML = `
            <div class="metric-title">Resumen del trayecto</div>
            <div class="metric-row">
                <div class="metric-label">Total kilómetros</div>
                <div class="metric-value">${distanceKm.toFixed(1)} km</div>
            </div>
            <div class="metric-row">
                <div class="metric-label">Tiempo estimado</div>
                <div class="metric-value">${timeString}</div>
            </div>
        `;
    }

    function drawFallbackRoute() {
        if (!selectedFromId || !selectedToId) return;
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
            lineCap: 'round',
            pane: 'routePane'
        }).addTo(map);

        const core = L.polyline(path, {
            color: '#0a84ff',
            weight: 5,
            opacity: 0.9,
            lineCap: 'round',
            pane: 'routePane'
        }).addTo(map);

        routeLayers.push(glow, core);
        map.fitBounds([path[0], path[1]], { padding: [80, 80] });

        const distance = getDistanceInKm(from, to);
        const timeMinutes = estimateTravelMinutes(distance);
        updateDistanceIndicator(distance, timeMinutes);
    }

    function getDistanceInKm(a, b) {
        const R = 6371;
        const dLat = (b.latitude - a.latitude) * Math.PI / 180;
        const dLon = (b.longitude - a.longitude) * Math.PI / 180;
        const lat1 = a.latitude * Math.PI / 180;
        const lat2 = b.latitude * Math.PI / 180;

        const sinLat = Math.sin(dLat / 2);
        const sinLon = Math.sin(dLon / 2);
        const c = 2 * Math.atan2(
            Math.sqrt(sinLat * sinLat + Math.cos(lat1) * Math.cos(lat2) * sinLon * sinLon),
            Math.sqrt(1 - sinLat * sinLat - Math.cos(lat1) * Math.cos(lat2) * sinLon * sinLon)
        );

        return R * c;
    }

    function estimateTravelMinutes(distanceKm) {
        const averageSpeedKmH = 60;
        return (distanceKm / averageSpeedKmH) * 60;
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

    function initializeRouteStops() {
        if (!routeList) return;
        refreshRouteStopRoles();

        if (addStopSelect) {
            addStopSelect.addEventListener('change', function () {
                if (!this.value) return;
                addIntermediateStop(this.value);
                this.selectedIndex = 0;
            });
        }
    }

    function refreshRouteStopRoles() {
        if (!routeList) return;
        const stops = Array.from(routeList.querySelectorAll('.route-stop'));
        stops.forEach((stop, index) => {
            stop.classList.remove('route-stop--origin', 'route-stop--destination', 'route-stop--stop');
            const label = stop.querySelector('label');
            const select = stop.querySelector('select');
            const removeButton = stop.querySelector('.route-stop-remove');
            select.removeAttribute('id');

            if (index === 0) {
                stop.classList.add('route-stop--origin');
                label.innerHTML = 'Origen <span class="required-indicator">*</span>';
                select.id = 'fromLocation';
                selectedFromId = select.value || null;
                if (removeButton) {
                    removeButton.style.visibility = 'hidden';
                }
            } else if (index === stops.length - 1) {
                stop.classList.add('route-stop--destination');
                label.innerHTML = 'Destino <span class="required-indicator">*</span>';
                select.id = 'toLocation';
                selectedToId = select.value || null;
                if (removeButton) {
                    removeButton.style.visibility = 'hidden';
                }
            } else {
                stop.classList.add('route-stop--stop');
                label.textContent = 'Parada';
                if (removeButton) {
                    removeButton.style.visibility = 'visible';
                }
            }

            attachStopEvents(stop);
        });

        attachRouteSelectListeners();
    }

    function attachRouteSelectListeners() {
        if (!routeList) return;
        const selects = routeList.querySelectorAll('.route-stop select');
        selects.forEach(select => {
            select.onchange = function () {
                if (this.id === 'fromLocation') {
                    selectedFromId = this.value;
                    focusLocationById(selectedFromId);
                } else if (this.id === 'toLocation') {
                    selectedToId = this.value;
                }
                drawRouteIfReady();
            };
        });
    }

    function addIntermediateStop(value) {
        if (!routeList) return;
        const stop = document.createElement('div');
        stop.className = 'route-stop route-stop--stop';
        stop.setAttribute('draggable', 'true');
        stop.innerHTML = `
            <div class="route-stop-body">
                <label>Parada</label>
            </div>
            <button type="button" class="route-stop-remove" aria-label="Eliminar parada">
                <span class="material-symbols-rounded">close</span>
            </button>
            <div class="route-stop-handle"></div>
        `;

        const select = document.createElement('select');
        select.innerHTML = locationOptionsHtml;
        select.value = value;
        select.required = true;
        stop.querySelector('.route-stop-body').appendChild(select);
        routeList.insertBefore(stop, routeList.lastElementChild);
        refreshRouteStopRoles();
        drawRouteIfReady();
    }

    function attachStopEvents(stop) {
        if (!stop || stop.dataset.bound) return;
        stop.addEventListener('dragstart', handleDragStart);
        stop.addEventListener('dragover', handleDragOver);
        stop.addEventListener('drop', handleDrop);
        stop.addEventListener('dragend', handleDragEnd);
        const removeButton = stop.querySelector('.route-stop-remove');
        if (removeButton) {
            removeButton.addEventListener('click', () => {
                if (routeList.children.length > 2) {
                    stop.remove();
                    refreshRouteStopRoles();
                    drawRouteIfReady();
                }
            });
        }
        stop.dataset.bound = 'true';
    }

    function handleDragStart(e) {
        draggedStop = e.currentTarget;
        e.dataTransfer.effectAllowed = 'move';
        e.currentTarget.classList.add('dragging');
    }

    function handleDragOver(e) {
        e.preventDefault();
        const target = e.currentTarget;
        if (!draggedStop || target === draggedStop) return;
        const rect = target.getBoundingClientRect();
        const offset = e.clientY - rect.top;
        const insertAfter = offset > rect.height / 2;
        routeList.insertBefore(draggedStop, insertAfter ? target.nextSibling : target);
    }

    function handleDrop(e) {
        e.preventDefault();
    }

    function handleDragEnd(e) {
        e.currentTarget.classList.remove('dragging');
        draggedStop = null;
        refreshRouteStopRoles();
        drawRouteIfReady();
    }

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

        distanceControl = document.querySelector('.distance-indicator');
        if (distanceControl) {
            distanceControl.innerHTML = distancePlaceholder;
        }

        initializeRouteStops();
        drawRouteIfReady();
    };

    map.on('zoomend', applyMarkerScale);
});
