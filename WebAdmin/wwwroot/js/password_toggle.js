document.addEventListener('DOMContentLoaded', function() {
    // 1. Lấy tất cả các nút toggle
    const toggleButtons = document.querySelectorAll('.toggle-password-btn');

    toggleButtons.forEach(toggleButton => {
        
        // 2. Tìm thẻ cha gần nhất (.password-wrapper) thay vì .input-field-wrapper
        const wrapper = toggleButton.closest('.password-wrapper');
        
        if (wrapper) {
            // 3. Tìm input bên trong wrapper đó (lấy thẻ input bất kỳ trong wrapper này)
            const passwordInput = wrapper.querySelector('input');
            
            // 4. Tìm icon bên trong nút button
            const visibilityIcon = toggleButton.querySelector('.icon-visibility');

            if (passwordInput && visibilityIcon) {
                
                // Đảm bảo icon đúng trạng thái ban đầu
                visibilityIcon.textContent = 'visibility'; 

                toggleButton.addEventListener('click', function() {
                    // Lấy type hiện tại
                    const currentType = passwordInput.getAttribute('type');
                    
                    // Chuyển đổi type
                    const newType = currentType === 'password' ? 'text' : 'password';
                    passwordInput.setAttribute('type', newType);

                    // Chuyển đổi Icon
                    // visibility: hình con mắt (đang hiện hoặc nút để hiện)
                    // visibility_off: hình con mắt gạch chéo (đang ẩn)
                    // Logic google font thường là: 
                    // Hiện pass (type=text) -> hiển thị icon 'visibility_off' (bấm để ẩn)
                    // Ẩn pass (type=password) -> hiển thị icon 'visibility' (bấm để hiện)
                    
                    if (newType === 'text') {
                        visibilityIcon.textContent = 'visibility_off'; 
                    } else {
                        visibilityIcon.textContent = 'visibility'; 
                    }
                });
            }
        }
    });
});