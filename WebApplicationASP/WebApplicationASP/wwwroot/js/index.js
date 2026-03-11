const cards = document.querySelectorAll('.activity_card_container');
const logicSwitch = document.getElementById('logic-switch');
const searchInput = document.getElementById('search-input');

function filterData() {
    const selectedStatuses = Array.from(document.querySelectorAll('.status-checkbox:checked'))
        .map(cb => cb.value);

    const selectedCategories = Array.from(document.querySelectorAll('.category-checkbox:checked'))
        .map(cb => cb.value);

    const isAndMode = logicSwitch.checked;
    const searchText = searchInput.value.toLowerCase().trim();

    cards.forEach(card => {
        const cardStatus = card.getAttribute('data-status');
        const cardCategories = card.getAttribute('data-category')
            ? card.getAttribute('data-category').split(',')
            : [];

        const isStatusMatch = selectedStatuses.length === 0 || selectedStatuses.includes(cardStatus);

        let isCategoryMatch = false;

        if (selectedCategories.length === 0) {
            isCategoryMatch = true;
        } else {
            if (isAndMode) {
                isCategoryMatch = selectedCategories.every(selected => cardCategories.includes(selected));
            } else {
                isCategoryMatch = selectedCategories.some(selected => cardCategories.includes(selected));
            }
        }

        const cardTitle = card.querySelector('.card-title').innerText.toLowerCase();
        const cardDescription = card.querySelector('.card-description').innerText.toLowerCase();

        const isSearchMatch = searchText === "" || cardTitle.includes(searchText) || cardDescription.includes(searchText);

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
    event.stopPropagation();

    const menu = document.getElementById("profileMenu");

    if (menu.style.display === "block") {
        menu.style.display = "none";
    } else {
        menu.style.display = "block";
    }
}

function toggleFilterMenu(event) {
    event.stopPropagation();

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


    if (profileMenu.style.display === "block" && !profileMenu.contains(event.target)) {
        profileMenu.style.display = "none";
    }

    if (filterMenu.style.display === "block" && !filterMenu.contains(event.target)) {
        filterMenu.style.display = "none";
    }
}

function openModal(element) {
    var activityId = element.getAttribute('data-id');

    document.getElementById('modalTitle').innerText = "กำลังโหลด...";

    fetch('/Activity/GetActivityDetails?id=' + activityId)
        .then(function (response) {
            if (!response.ok) throw new Error('เชื่อมต่อไม่สำเร็จ');
            return response.json();
        })
        .then(function (data) {
            document.getElementById('modalTitle').innerText = data.title;
            document.getElementById('modalHost').innerText = data.host;
            document.getElementById('modalDescription').innerText = data.description;
            document.getElementById('modalExpireDate').innerText = data.expireDate;
            document.getElementById('modalMemberCount').innerText = data.memberCount;
            document.getElementById('modalEmptySeats').innerText = data.emptySeats;
            document.getElementById('modalmaxParticipants').innerText = data.maxParticipants;
            document.getElementById('modalImage').src = data.imageUrl;

            var membersList = document.getElementById('modalMembersList');
            membersList.innerHTML = "";

            if (data.members && data.members.length > 0) {
                data.members.forEach(function (name) {
                    var li = document.createElement("li");
                    li.innerText = name;
                    membersList.appendChild(li);
                });
                document.getElementById('toggleMembersBtn').style.display = "inline-block";
            } else {
                document.getElementById('toggleMembersBtn').style.display = "none";
            }
            document.getElementById('membersListContainer').style.display = "none";

            var categoryString = data.category;
            if (categoryString && categoryString !== "ไม่มีหมวดหมู่") {
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

            var statusElement = document.getElementById('modalStatus');
            statusElement.innerText = data.status;

            if (data.status.toLowerCase() === "open") {
                statusElement.style.color = "green";
                statusElement.style.fontWeight = "bold";
            } else {
                statusElement.style.color = "red";
                statusElement.style.fontWeight = "bold";
            }

            var joinBtn = document.getElementById('joinActivityBtn');
            var editBtn = document.getElementById('editActivityBtn');
            joinBtn.setAttribute('data-id', activityId);

            if (data.isHost) {
                if (data.status.toLowerCase() === "close") {
                    editBtn.style.display = "none";

                    joinBtn.style.display = "block";
                    joinBtn.innerText = "กิจกรรมนี้ถูกปิดแล้ว";
                    joinBtn.style.backgroundColor = "#ccc";
                    joinBtn.disabled = true;
                    joinBtn.style.cursor = "not-allowed";
                } else {
                    editBtn.style.display = "block";
                    editBtn.href = "/Activity/EditActivity?id=" + data.id;
                    joinBtn.style.display = "none";
                }
            } else {
                editBtn.style.display = "none";
                joinBtn.style.display = "block";

                if (data.hasJoined) {
                    joinBtn.innerText = "เข้าร่วมแล้ว";
                    joinBtn.style.backgroundColor = "#81c784";
                    joinBtn.disabled = true;
                    joinBtn.style.cursor = "not-allowed";
                }
                else if (data.status.toLowerCase() === "close" || data.emptySeats <= 0) {
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

function closeModal() {
    document.getElementById('customModal').style.display = "none";
}

window.onclick = function (event) {
    var modal = document.getElementById('customModal');
    if (event.target == modal) {
        modal.style.display = "none";
    }
}

async function joinActivity() {
    var btn = document.getElementById('joinActivityBtn');
    var activityId = btn.getAttribute('data-id');

    try {
        const response = await fetch('/Activity/JoinActivity', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ id: parseInt(activityId) })
        });

        const result = await response.json();

        if (response.ok && result.success) {
            alert("เข้าร่วมกิจกรรมสำเร็จ!");
            window.location.reload();
        } else {
            alert("ไม่สามารถเข้าร่วมได้: " + (result.message || "เกิดข้อผิดพลาด"));
        }
    } catch (error) {
        console.error('Error:', error);
        alert("เกิดข้อผิดพลาดในการเชื่อมต่อ กรุณาล็อกอินก่อนทำรายการ");
    }
}

function toggleMembers() {
    var container = document.getElementById('membersListContainer');
    if (container.style.display === "none") {
        container.style.display = "block";
    } else {
        container.style.display = "none";
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

                const profileImg = document.querySelector('.profile_img');
                if (profileImg) profileImg.src = data.newImageUrl;

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

}
 
