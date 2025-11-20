(() => {
    const seatMapContainer = document.getElementById('seatMap');
    const landVehicleSelect = document.getElementById('landVehicle');
    const landSeatsInput = document.getElementById('landSeats');
    let selectedSeatNumbers = new Set();

    const layouts = {
        camioneta: {
            max: 6,
            segments: [
                {
                    name: 'Camioneta',
                    rows: [
                        ['driver', '1', '2'],
                        ['aisle', '3', '4'],
                        ['aisle', '5', '6']
                    ]
                }
            ]
        },
        van: {
            max: 12,
            segments: [
                {
                    name: 'Van',
                    rows: [
                        ['driver', '1', '2', '3'],
                        ['aisle', '4', '5', '6'],
                        ['aisle', '7', '8', '9'],
                        ['aisle', '10', '11', '12']
                    ]
                }
            ]
        },
        autobus: {
            max: 40,
            segments: [
                {
                    name: 'Autobús',
                    rows: [
                        ['driver', '1', '2', '3', '4'],
                        ['aisle', '5', '6', '7', '8'],
                        ['aisle', '9', '10', '11', '12'],
                        ['aisle', '13', '14', '15', '16'],
                        ['aisle', '17', '18', '19', '20'],
                        ['aisle', '21', '22', '23', '24'],
                        ['aisle', '25', '26', '27', '28'],
                        ['aisle', '29', '30', '31', '32'],
                        ['aisle', '33', '34', '35', '36'],
                        ['aisle', '37', '38', '39', '40']
                    ]
                }
            ]
        }
    };

    function renderSeats() {
        if (!seatMapContainer || !landVehicleSelect || !landSeatsInput) return;
        const type = landVehicleSelect.value || 'camioneta';
        const layout = layouts[type];
        seatMapContainer.innerHTML = '';

        let requiredSeats = parseInt(landSeatsInput.value, 10) || 0;
        requiredSeats = Math.min(Math.max(requiredSeats, 0), 999);
        const vehiclesNeeded = Math.max(1, Math.ceil(Math.max(requiredSeats, 1) / layout.max));
        selectedSeatNumbers.forEach((seat, idx) => {
            if (idx >= requiredSeats) {
                selectedSeatNumbers.delete(seat);
            }
        });

        let seatCounter = 0;

        for (let v = 0; v < vehiclesNeeded; v++) {
            const segmentTitle = document.createElement('div');
            segmentTitle.className = 'seat-segment-title';
            segmentTitle.textContent = `${layout.segments[0].name} ${v + 1}`;
            seatMapContainer.appendChild(segmentTitle);

            const segment = document.createElement('div');
            segment.className = 'seat-segment';
            seatMapContainer.appendChild(segment);
            segment.style.gridTemplateColumns = `repeat(${layout.segments[0].rows[0].length}, minmax(28px, 1fr))`;

            layout.segments[0].rows.forEach(row => {
                row.forEach(cell => {
                    const seatElement = document.createElement('div');
                    if (cell === 'aisle') {
                        seatElement.className = 'seat aisle';
                        segment.appendChild(seatElement);
                        return;
                    }

                    seatCounter++;
                    const seatNumber = `${seatCounter}`;
                    
                    seatElement.className = cell === 'driver' ? 'seat driver' : 'seat';
                    seatElement.textContent = cell === 'driver' ? 'D' : seatNumber;
                    seatElement.dataset.seatNumber = seatNumber;

                    if (cell !== 'driver') {
                        if (selectedSeatNumbers.has(seatNumber)) {
                            seatElement.classList.add('selected');
                        }
                        seatElement.addEventListener('click', () => {
                            if (seatElement.classList.contains('selected')) {
                                seatElement.classList.remove('selected');
                                selectedSeatNumbers.delete(seatNumber);
                            } else {
                                seatElement.classList.add('selected');
                                selectedSeatNumbers.add(seatNumber);
                            }
                            landSeatsInput.value = selectedSeatNumbers.size;
                        });
                    } else {
                        seatElement.classList.add('disabled');
                    }

                    segment.appendChild(seatElement);
                });
            });
        }

        autoSelectSeats(requiredSeats);
    }

    function autoSelectSeats(requiredSeats) {
        const seats = seatMapContainer.querySelectorAll('.seat:not(.driver):not(.aisle)');
        selectedSeatNumbers.clear();
        seats.forEach(seat => seat.classList.remove('selected'));
        for (let i = 0; i < requiredSeats && i < seats.length; i++) {
            const seatNumber = seats[i].dataset.seatNumber;
            seats[i].classList.add('selected');
            selectedSeatNumbers.add(seatNumber);
        }
        landSeatsInput.value = requiredSeats;
    }

    landVehicleSelect?.addEventListener('change', renderSeats);
    landSeatsInput?.addEventListener('input', renderSeats);
    document.addEventListener('DOMContentLoaded', renderSeats);
})();
