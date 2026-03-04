

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

const openBtn = document.getElementById('openFilter');
const filterMenu = document.getElementById('filterMenu');
const closeBtn = document.querySelector('.close-btn');

// เปิดเมนู
openBtn.onclick = () => {
  filterMenu.classList.add('active');
};

// ปิดเมนู
closeBtn.onclick = () => {
  filterMenu.classList.remove('active');
};

// ปิดเมื่อกดข้างนอกเมนู (Optional)
window.onclick = (event) => {
  if (event.target == filterMenu) {
    filterMenu.classList.remove('active');
  }
};

// เรียกตอนโหลดหน้า
document.addEventListener("DOMContentLoaded", () => {
    loadEvents();
});

async function loadEvents() {
    // try {
    //     const events = await getEvents(); // มาจาก api.js
    //     renderEvents(events);
    // } catch (err) {
    //     console.error("โหลดกิจกรรมล้มเหลว:", err);
    //     document.getElementById("event-list").innerHTML =
    //         "<p>ไม่สามารถโหลดกิจกรรมได้</p>";
    // }
}

function renderEvents(events) {
    const container = document.getElementById("event-list");
    container.innerHTML = "";

    if (!events || events.length === 0) {
        container.innerHTML = "<p>ยังไม่มีกิจกรรม</p>";
        return;
    }

    events.forEach(event => {

        const isFull = event.currentParticipants >= event.maxParticipants;
        const isExpired = new Date(event.date) < new Date();
        const isClosed = isFull || isExpired;

        const statusClass = isClosed ? "status_close" : "status_open";
        const statusText = isClosed ? "Status: Close" : "Status: Open";

        const cardHTML = `
            ${!isClosed ? `
            <a href="detail.html?id=${event.id}" class="activity_card_link">
            ` : ""}
            
            <div class="activity_card">
                <img src="${event.imageUrl || 'cat.jpg'}" 
                     alt="${event.title}" 
                     class="activity_img" />

                <div class="activity_info">
                    <h3>${event.title}</h3>
                    <p>${truncateText(event.description, 80)}</p>
                    <span class="status ${statusClass}">
                        ${statusText}
                    </span>
                </div>
            </div>

            ${!isClosed ? `</a>` : ""}
        `;

        container.innerHTML += cardHTML;
    });
}

// ตัดข้อความไม่ให้ยาวเกิน
function truncateText(text, maxLength) {
    if (!text) return "";
    return text.length > maxLength 
        ? text.substring(0, maxLength) + "..."
        : text;
}

async function handleJoin(id) {
    const res = await joinEvent(id);

    if(res.ok) {
        alert("เข้าร่วมสำเร็จ");
        loadEvents();
    } else {
        const msg = await res.text();
        alert(msg);
    }
}