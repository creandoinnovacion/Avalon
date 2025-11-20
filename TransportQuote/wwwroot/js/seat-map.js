(() => {
    const seatMapContainer = document.getElementById('seatMap');
    const landVehicleSelect = document.getElementById('landVehicle');
    const landSeatsInput = document.getElementById('landSeats');

    const layouts = {
        camioneta: {
            max: 6,
            rows: [
                ['driver', '1', '2'],
                ['aisle', '3', '4'],
                ['aisle', '5', '6']
            ]
        },
        van: {
            max: 12,
            rows: [
                ['driver', '1', '2', '3'],
                ['aisle', '4', '5', '6'],
                ['aisle', '7', '8', '9'],
                ['aisle', '10', '11', '12']
            ]
        },
        autobus: {
            max: 40,
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
    };

    function renderSeats() {
        if (!seatMapContainer || !landVehicleSelect || !landSeatsInput) return;
        const type = landVehicleSelect.value || 'camioneta';
        const layout = layouts[type];
        seatMapContainer.innerHTML = '';

        landSeatsInput.max = layout.max;
        if (parseInt(landSeatsInput.value, 10) > layout.max) {
            landSeatsInput.value = layout.max;
        }

        const selectedSeats = new Set();
        seatMapContainer.style.gridTemplateColumns = `repeat(${layout.rows[0].length}, minmax(28px, 1fr))`;

        layout.rows.forEach(row => {
            row.forEach(cell => {
                const seatElement = document.createElement('div');
                if (cell === 'aisle') {
                    seatElement.className = 'seat aisle';
                    seatMapContainer.appendChild(seatElement);
                    return;
                }

                seatElement.className = cell === 'driver' ? 'seat driver' : 'seat';
                seatElement.textContent = cell === 'driver' ? 'D' : cell;
                seatElement.dataset.seatNumber = cell;

                if (cell !== 'driver') {
                    seatElement.addEventListener('click', () => {
                        seatElement.classList.toggle('selected');
                        if (seatElement.classList.contains('selected')) {
                            selectedSeats.add(cell);
                        } else {
                            selectedSeats.delete(cell);
                        }
                        landSeatsInput.value = selectedSeats.size;
                    });
                } else {
                    seatElement.classList.add('disabled');
                }

                seatMapContainer.appendChild(seatElement);
            });
        });

        landSeatsInput.addEventListener('input', () => syncSeatSelection(selectedSeats));
        syncSeatSelection(selectedSeats);
    }

    function syncSeatSelection(selectedSeats) {
        const seats = seatMapContainer.querySelectorAll('.seat');
        const limit = Math.min(parseInt(landSeatsInput.value, 10) || 0, parseInt(landSeatsInput.max, 10));
        landSeatsInput.value = limit;
        selectedSeats.clear();
        seats.forEach(seat => {
            if (!seat.dataset.seatNumber || seat.classList.contains('driver')) {
                seat.classList.remove('selected');
                return;
            }
            seat.classList.remove('selected');
        });
        let count = 0;
        seats.forEach(seat => {
            if (count >= limit) return;
            if (seat.dataset.seatNumber && !seat.classList.contains('driver')) {
                seat.classList.add('selected');
                selectedSeats.add(seat.dataset.seatNumber);
                count++;
            }
        });
    }

    landVehicleSelect?.addEventListener('change', () => renderSeats());
    document.addEventListener('DOMContentLoaded', renderSeats);
})();
