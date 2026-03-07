
// const openBtn = document.getElementById('openFilter');
// const filterMenu = document.getElementById('filterMenu');
// const closeBtn = document.querySelector('.close-btn');

// // เปิดเมนู
// openBtn.onclick = (e) => {
//     filterMenu.classList.add('active');
// };

// // ปิดเมนู
// closeBtn.onclick = () => {
//     filterMenu.classList.remove('active');
// };

// // ปิดเมื่อกดข้างนอกเมนู (Optional)
// window.onclick = (event) => {
//     if (event.target == filterMenu) {
//         filterMenu.classList.remove('active');
//     }
// };

const applyBtn = document.querySelector('.apply-btn');
const cards = document.querySelectorAll('.activity_card_link');
const logicSwitch = document.getElementById('logic-switch'); // ดึง ID สวิตช์มา

applyBtn.addEventListener('click', () => {
    // 1. ดึงค่าจาก Checkbox
    const selectedStatuses = Array.from(document.querySelectorAll('.status-checkbox:checked'))
        .map(cb => cb.value);

    const selectedCategories = Array.from(document.querySelectorAll('.category-checkbox:checked'))
        .map(cb => cb.value);

    // เช็กสถานะสวิตช์: true = AND (ต้องครบ), false = OR (อย่างใดอย่างหนึ่ง)
    const isAndMode = logicSwitch.checked;

    // 2. วนลูปตรวจสอบ Card
    cards.forEach(card => {
        const cardStatus = card.getAttribute('data-status');
        const cardCategories = card.getAttribute('data-category') 
            ? card.getAttribute('data-category').split(',') 
            : [];

        // เงื่อนไขสถานะ (เหมือนเดิม)
        const isStatusMatch = selectedStatuses.length === 0 || selectedStatuses.includes(cardStatus);

        // --- ส่วนที่แก้ไข: เงื่อนไขหมวดหมู่ ---
        let isCategoryMatch = false;

        if (selectedCategories.length === 0) {
            isCategoryMatch = true; // ถ้าไม่เลือกเลย ให้ผ่าน
        } else {
            if (isAndMode) {
                // โหมด AND: ทุกค่าที่เลือก (selected) ต้องมีอยู่ใน cardCategories
                isCategoryMatch = selectedCategories.every(selected => cardCategories.includes(selected));
            } else {
                // โหมด OR: ขอแค่บางค่าที่เลือก (selected) มีอยู่ใน cardCategories (Code เดิมของคุณ)
                isCategoryMatch = selectedCategories.some(selected => cardCategories.includes(selected));
            }
        }

        // 3. แสดงผลเฉพาะ Card ที่ผ่าน "ทั้งสองกลุ่มเงื่อนไข"
        if (isStatusMatch && isCategoryMatch) {
            card.style.display = 'flex';
        } else {
            card.style.display = 'none';
        }
    });
});

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

    if (res.ok) {
        alert("เข้าร่วมสำเร็จ");
        loadEvents();
    } else {
        const msg = await res.text();
        alert(msg);
    }
}

async function handleLogin(event) {
    event.preventDefault();

    const email = document.getElementById("email").value;
    const password = document.getElementById("password").value;

    const response = await fetch("/auth/login", {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify({
            email: email,
            password: password
        })
    });

    const text = await response.text();
    alert(text);

    if (response.ok) {
        window.location.href = "/";
    }
}

async function handleRegister(event) {
    event.preventDefault()

    const username = document.getElementById("username").value
    const email = document.getElementById("email").value
    const password = document.getElementById("password").value

    const res = await fetch("/auth/signup", {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify({
            username: username,
            email: email,
            password: password
        })
    })

    const text = await res.text()
    alert(text)

    if (res.ok) {
        window.location.href = "/auth/login"
    }
}

function toggleProfileMenu(event) {
    event.stopPropagation(); // สำคัญมาก

    const menu = document.getElementById("profileMenu");

    if (menu.style.display === "block") {
        menu.style.display = "none";
    } else {
        menu.style.display = "block";
    }
}
// window.addEventListener("click", function (event) {

//     // -------- profile menu --------
//     const menu = document.getElementById("profileMenu");
//     menu.style.display = "none";

//     // -------- filter menu --------
//     const filterMenu = document.getElementById("filterMenu");

//     if (event.target === filterMenu) {
//         filterMenu.classList.remove("active");
//     }

// });

function toggleFilterMenu(event) {
    event.stopPropagation(); // สำคัญมาก

    const filtermenu = document.getElementById("filterMenu");

    if (filtermenu.style.display === "block") {
        filtermenu.style.display = "none";
    } else {
        filtermenu.style.display = "block";
    }
}