
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

const cards = document.querySelectorAll('.activity_card_container');
const logicSwitch = document.getElementById('logic-switch'); // ดึง ID สวิตช์มา
const searchInput = document.getElementById('search-input');

function filterData() {
    // 1. ดึงค่าจาก Checkbox
    const selectedStatuses = Array.from(document.querySelectorAll('.status-checkbox:checked'))
        .map(cb => cb.value);

    const selectedCategories = Array.from(document.querySelectorAll('.category-checkbox:checked'))
        .map(cb => cb.value);

    // เช็กสถานะสวิตช์: true = AND (ต้องครบ), false = OR (อย่างใดอย่างหนึ่ง)
    const isAndMode = logicSwitch.checked;
    const searchText = searchInput.value.toLowerCase().trim();

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

        const cardTitle = card.querySelector('.card-title').innerText.toLowerCase();
        const cardDescription = card.querySelector('.card-description').innerText.toLowerCase();

        const isSearchMatch = searchText === "" || cardTitle.includes(searchText) || cardDescription.includes(searchText);

        // 3. แสดงผลเฉพาะ Card ที่ผ่าน "ทั้งสองกลุ่มเงื่อนไข"
        if (isStatusMatch && isCategoryMatch && isSearchMatch) {
            card.style.display = 'flex';
        } else {
            card.style.display = 'none';
        }
    });
}

searchInput.addEventListener('input', () => {
    filterData()
});

logicSwitch.addEventListener('change', () => {
    filterData()
});

document.addEventListener('change', (e) => {
    if (e.target.classList.contains('status-checkbox') ||
        e.target.classList.contains('category-checkbox')) {
        filterData();
    }
});

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

function toggleFilterMenu(event) {
    event.stopPropagation(); // สำคัญมาก

    const filtermenu = document.getElementById("filterMenu");

    if (filtermenu.style.display === "block") {
        filtermenu.style.display = "none";
    } else {
        filtermenu.style.display = "block";
    }
}

window.onclick = function (event) {
    const profileMenu = document.getElementById("profileMenu");
    const filterMenu = document.getElementById("filterMenu");

    // ตรวจสอบว่าจุดที่คลิก (event.target) ไม่ใช่ตัวเมนู และไม่ใช่ปุ่มกด
    // ถ้าคลิกข้างนอก ให้สั่งปิดเมนู (display = "none")

    if (profileMenu.style.display === "block" && !profileMenu.contains(event.target)) {
        profileMenu.style.display = "none";
    }

    if (filterMenu.style.display === "block" && !filterMenu.contains(event.target)) {
        filterMenu.style.display = "none";
    }
}

// --- โค้ดสำหรับทำ AJAX และ Popup Modal (Vanilla JS) ---

// 1. ฟังก์ชันเปิด Modal และดึงข้อมูล
function openModal(element) {
    var activityId = element.getAttribute('data-id');

    // ขึ้นข้อความโหลดรอไว้ก่อน
    document.getElementById('modalTitle').innerText = "กำลังโหลด...";

    fetch('/Activity/GetActivityDetails?id=' + activityId)
        .then(function (response) {
            if (!response.ok) throw new Error('เชื่อมต่อไม่สำเร็จ');
            return response.json();
        })
        .then(function (data) {
            // เอาข้อมูลใส่ใน Modal ให้ตรงกับ ID ใหม่
            document.getElementById('modalTitle').innerText = data.title;
            document.getElementById('modalHost').innerText = data.host;
            document.getElementById('modalDescription').innerText = data.description;
            document.getElementById('modalExpireDate').innerText = data.expireDate;
            document.getElementById('modalMemberCount').innerText = data.memberCount;
            document.getElementById('modalEmptySeats').innerText = data.emptySeats;
            document.getElementById('modalmaxParticipants').innerText = data.maxParticipants;
            document.getElementById('modalImage').src = data.imageUrl;

            // รายชื่อผู้สมัคร
            var membersList = document.getElementById('modalMembersList');
            membersList.innerHTML = ""; // ล้างค่าเก่าออกก่อน

            // ตรวจสอบว่ามีคนสมัครหรือไม่
            if (data.members && data.members.length > 0) {
                // ถ้ามีคนสมัคร ให้วนลูปสร้าง <li> แล้วใส่ชื่อลงไป
                data.members.forEach(function (name) {
                    var li = document.createElement("li");
                    li.innerText = name;
                    membersList.appendChild(li);
                });
                document.getElementById('toggleMembersBtn').style.display = "inline-block"; // แสดงปุ่ม
            } else {
                document.getElementById('toggleMembersBtn').style.display = "none"; // ถ้าไม่มีใครสมัคร ให้ซ่อนปุ่ม
            }
            // ปิดกล่องรายชื่อไว้ก่อนเสมอตอนเปิดหน้า Modal
            document.getElementById('membersListContainer').style.display = "none";

            // --- เพิ่มการใส่อีโมจิ ----
            var categoryString = data.category;
            if (categoryString && categoryString !== "ไม่มีหมวดหมู่") {
                // แยกประเภทด้วยลูกน้ำ (กรณีมีหลายประเภท)
                var categories = categoryString.split(',').map(function (c) { return c.trim().toLowerCase(); });

                var categoryWithEmojis = categories.map(function (c) {
                    if (c === 'sport') return 'กีฬา ⚽';
                    if (c === 'music') return 'ดนตรี 🎵 ';
                    if (c === 'food') return 'อาหาร 🍔';
                    return 'ทั่วไป 💬';
                });

                document.getElementById('modalCategory').innerText = categoryWithEmojis.join(', ');
            } else {
                document.getElementById('modalCategory').innerText = categoryString;
            }

            // --- เพิ่มเงื่อนไขเช็คสถานะ + เปลี่ยนสี ---
            var statusElement = document.getElementById('modalStatus');
            statusElement.innerText = data.status; // ใส่ข้อความสถานะ

            if (data.status.toLowerCase() === "open") {
                statusElement.style.color = "green"; // ถ้า open ให้เป็นสีเขียว
                statusElement.style.fontWeight = "bold"; // ทำให้ตัวหนาขึ้น (ถ้าต้องการ)
            } else {
                statusElement.style.color = "red";   // ถ้าเป็นอย่างอื่น (close) ให้เป็นสีแดง
                statusElement.style.fontWeight = "bold";
            }

            // --- ส่วนจัดการปุ่ม Join และ Edit ---
            var joinBtn = document.getElementById('joinActivityBtn');
            var editBtn = document.getElementById('editActivityBtn');
            joinBtn.setAttribute('data-id', activityId);

            if (data.isHost) {
                // ==========================================
                // กรณีที่เป็น "เจ้าของกิจกรรม"
                // ==========================================
                if (data.status.toLowerCase() === "close") {
                    // ถ้ากิจกรรมถูกปิดไปแล้ว
                    editBtn.style.display = "none"; // ซ่อนปุ่มแก้ไข

                    // นำปุ่ม Join มาใช้แสดงข้อความว่าปิดแล้วแทน (กดไม่ได้)
                    joinBtn.style.display = "block";
                    joinBtn.innerText = "กิจกรรมนี้ถูกปิดแล้ว";
                    joinBtn.style.backgroundColor = "#ccc"; // สีเทา
                    joinBtn.disabled = true;
                    joinBtn.style.cursor = "not-allowed";
                } else {
                    // ถ้ากิจกรรมยังเปิดอยู่
                    editBtn.style.display = "block"; // โชว์ปุ่มแก้ไข
                    editBtn.href = "/Activity/EditActivity?id=" + data.id;
                    joinBtn.style.display = "none";  // ซ่อนปุ่ม Join
                }
            } else {
                // ==========================================
                // กรณีที่ "ไม่ใช่เจ้าของกิจกรรม"
                // ==========================================
                editBtn.style.display = "none";  // ซ่อนปุ่มแก้ไขเสมอ
                joinBtn.style.display = "block"; // โชว์ปุ่ม Join

                // ควบคุมสถานะของปุ่ม Join
                if (data.hasJoined) {
                    joinBtn.innerText = "เข้าร่วมแล้ว";
                    joinBtn.style.backgroundColor = "#81c784";
                    joinBtn.disabled = true;
                    joinBtn.style.cursor = "not-allowed";
                }
                else if (data.status.toLowerCase() === "close" || data.emptySeats <= 0) {
                    // เพิ่มเช็คด้วยว่าถ้าสถานะ close ให้ขึ้นว่าเต็มแล้ว/ปิดแล้ว
                    joinBtn.innerText = data.status.toLowerCase() === "close" ? "กิจกรรมนี้ถูกปิดแล้ว" : "กิจกรรมเต็มแล้ว";
                    joinBtn.style.backgroundColor = "#ccc";
                    joinBtn.disabled = true;
                    joinBtn.style.cursor = "not-allowed";
                }
                else {
                    joinBtn.innerText = "Join กิจกรรม";
                    joinBtn.style.backgroundColor = "#28a745";
                    joinBtn.disabled = false;
                    joinBtn.style.cursor = "pointer";
                }
            }

            document.getElementById('customModal').style.display = "block";
        })
        .catch(function (error) {
            console.error('Error:', error);
            alert('ไม่สามารถดึงข้อมูลได้');
        });
}

// 2. ฟังก์ชันปิด Modal (กดปุ่มกากบาท)
function closeModal() {
    document.getElementById('customModal').style.display = "none";
}

// 3. ปิด Modal เมื่อผู้ใช้คลิกพื้นที่มืดๆ นอกกรอบป๊อปอัป
window.onclick = function (event) {
    var modal = document.getElementById('customModal');
    if (event.target == modal) {
        modal.style.display = "none";
    }
}

// ฟังก์ชันสำหรับกดปุ่ม Join
async function joinActivity() {
    var btn = document.getElementById('joinActivityBtn');
    var activityId = btn.getAttribute('data-id'); // ดึง ID กิจกรรม

    try {
        const response = await fetch('/Activity/JoinActivity', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            // ส่ง ID กิจกรรมไปให้ Backend
            body: JSON.stringify({ id: parseInt(activityId) })
        });

        const result = await response.json();

        if (response.ok && result.success) {
            alert("เข้าร่วมกิจกรรมสำเร็จ!");
            window.location.reload(); // รีโหลดหน้าเพื่ออัปเดตข้อมูลจำนวนคน
        } else {
            alert("ไม่สามารถเข้าร่วมได้: " + (result.message || "เกิดข้อผิดพลาด"));
        }
    } catch (error) {
        console.error('Error:', error);
        alert("เกิดข้อผิดพลาดในการเชื่อมต่อ กรุณาล็อกอินก่อนทำรายการ");
    }
}

// ฟังก์ชันสำหรับกดปุ่ม "ดูรายชื่อ"
function toggleMembers() {
    var container = document.getElementById('membersListContainer');
    if (container.style.display === "none") {
        container.style.display = "block"; // แสดงรายชื่อ
    } else {
        container.style.display = "none"; // ซ่อนรายชื่อ
    }
}

function uploadProfileImage(input) {
    if (!input.files || !input.files[0]) return;

    const file = input.files[0];
    const formData = new FormData();
    formData.append("profileImage", file);

    const status = document.getElementById('uploadStatus');
    status.innerText = "กำลังอัพโหลด...";

    fetch('/User/UploadProfileImage', {
        method: 'POST',
        body: formData
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                status.innerText = "อัพโหลดสำเร็จ!";

                // 1. เปลี่ยนรูปใหญ่ในหน้า Profile
                const profileImg = document.querySelector('.profile_img');
                if (profileImg) profileImg.src = data.newImageUrl;

                // 2. เปลี่ยนรูปจิ๋วใน Navbar ทันที (ไม่ต้องรอ Login ใหม่)
                const navImg = document.querySelector('.nav-profile-img');
                if (navImg) {
                    navImg.src = data.newImageUrl;
                }

                console.log("Updated Navbar Image to: " + data.newImageUrl);

            } else {
                status.innerText = "เกิดข้อผิดพลาด: " + data.message;
            }
        })
        .catch(error => {
            console.error('Error:', error);
            status.innerText = "เชื่อมต่อเซิร์ฟเวอร์ไม่ได้";
        });

    // --- อย่าเอาโค้ดมาวางตรงนี้ เพราะ data จะมองไม่เห็น (undefined) ---
}