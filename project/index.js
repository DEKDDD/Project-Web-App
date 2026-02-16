

const btn_available = document.getElementById('btn_available');
const btn_close = document.getElementById('btn_close');
const cards = document.querySelectorAll('.activity_card_link');


function filterActivities(status) {
    cards.forEach(card => {

        const cardStatus = card.querySelector('.status').textContent.toLowerCase();

        if (status === 'open' && cardStatus.includes('open')) {
            card.style.display = 'flex';
        }
        else if (status === 'close' && cardStatus.includes('close')) {
            card.style.display = 'flex';
        }
        else {
            card.style.display = 'none';
        }
    });
}


btn_available.addEventListener('click', () => filterActivities('open'));
btn_close.addEventListener('click', () => filterActivities('close'));
