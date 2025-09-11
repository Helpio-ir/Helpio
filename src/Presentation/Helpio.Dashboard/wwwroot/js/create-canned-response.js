// CreateCannedResponse JavaScript Module
(function() {
    'use strict';
    
    console.log('Loading CreateCannedResponse Module...');
    
    // Templates
    var templates = {
        greeting: "سلام {نام_مشتری} عزیز،\n\nامیدوارم حال شما خوب باشد. با تشکر از تماس شما با تیم پشتیبانی ما. چطور می‌توانم به شما کمک کنم؟\n\nبا احترام،\n{نام_کارشناس}",
        closing: "در صورت داشتن سوال یا مشکل اضافی، لطفاً با ما در تماس باشید. ما همیشه در خدمت شما هستیم.\n\nبا تشکر از صبر و شکیبایی شما،\n{نام_کارشناس}\nتیم پشتیبانی",
        investigation: "سلام {نام_مشتری} عزیز،\n\nبرای بررسی دقیق‌تر مشکل شما و ارائه بهترین راه‌حل، لطفاً اطلاعات زیر را ارسال کنید:\n\n• توضیح کامل مشکل\n• تصاویر یا فایل‌های مرتبط\n• زمان وقوع مشکل\n\nبا تشکر،\n{نام_کارشناس}",
        solution: "سلام {نام_مشتری} عزیز،\n\nراه‌حل مشکل شما به شرح زیر است:\n\n1. مرحله اول: ...\n2. مرحله دوم: ...\n3. مرحله سوم: ...\n\nدر صورت عدم حل مشکل یا نیاز به راهنمایی بیشتر، لطفاً مجدداً تماس بگیرید.\n\nموفق باشید،\n{نام_کارشناس}",
        follow_up: "سلام {نام_مشتری} عزیز،\n\nامیدوارم مشکل شما حل شده باشد. در صورت نیاز به پیگیری یا داشتن سوال جدید، لطفاً با ما در تماس باشید.\n\nهمچنین نظر شما در خصوص کیفیت خدمات ما برای ما بسیار ارزشمند است.\n\nبا تشکر،\n{نام_کارشناس}",
        apologize: "سلام {نام_مشتری} عزیز،\n\nبابت ناراحتی و مشکلی که برای شما ایجاد شده، پوزش می‌طلبیم. ما تمام تلاش خود را برای حل سریع این مسئله و جلوگیری از تکرار آن خواهیم کرد.\n\nصبر و درک شما را قدردانی می‌کنیم.\n\nبا عذرخواهی مجدد،\n{نام_کارشناس}"
    };
    
    // Global functions - isolated and protected
    function defineGlobalFunctions() {
        window.insertVariable = function(variable) {
            console.log('insertVariable called:', variable);
            var textarea = document.getElementById('responseContent');
            if (textarea) {
                var pos = textarea.selectionStart;
                var val = textarea.value;
                textarea.value = val.slice(0, pos) + variable + val.slice(textarea.selectionEnd);
                textarea.setSelectionRange(pos + variable.length, pos + variable.length);
                textarea.focus();
                if (window.updatePreview) window.updatePreview();
            }
        };
        
        window.addTag = function(tag) {
            console.log('addTag called:', tag);
            var input = document.getElementById('tagsInput');
            if (input) {
                var current = input.value.trim();
                var tags = current ? current.split(',').map(function(t) { return t.trim(); }) : [];
                if (tags.indexOf(tag) === -1) {
                    input.value = current ? current + ', ' + tag : tag;
                }
            }
        };
        
        window.useTemplate = function(name) {
            console.log('useTemplate called:', name);
            var textarea = document.getElementById('responseContent');
            if (textarea && templates[name]) {
                if (!textarea.value.trim() || confirm('آیا می‌خواهید محتوای فعلی جایگزین شود؟')) {
                    textarea.value = templates[name];
                    if (window.updatePreview) window.updatePreview();
                }
            }
        };
        
        window.updatePreview = function() {
            var textarea = document.getElementById('responseContent');
            var preview = document.getElementById('previewContent');
            if (textarea && preview) {
                var content = textarea.value;
                if (content.trim()) {
                    var html = content
                        .replace(/{نام_مشتری}/g, '<span class="text-primary fw-bold">احمد محمدی</span>')
                        .replace(/{شماره_تیکت}/g, '<span class="text-info fw-bold">#12345</span>')
                        .replace(/{نام_شرکت}/g, '<span class="text-success fw-bold">شرکت نمونه</span>')
                        .replace(/{تاریخ_امروز}/g, '<span class="text-warning fw-bold">' + new Date().toLocaleDateString('fa-IR') + '</span>')
                        .replace(/{نام_کارشناس}/g, '<span class="text-secondary fw-bold">علی رضایی</span>')
                        .replace(/\n/g, '<br>');
                    preview.innerHTML = '<div>' + html + '</div>';
                } else {
                    preview.innerHTML = '<div class="text-muted text-center"><i class="bi bi-eye" style="font-size: 2rem;"></i><br>شروع به نوشتن کنید تا پیش‌نمایش نمایش داده شود</div>';
                }
            }
        };
        
        window.testResponse = function() {
            console.log('testResponse called');
            var textarea = document.getElementById('responseContent');
            var nameInput = document.querySelector('input[name="Name"]');
            
            if (!textarea || !textarea.value.trim()) {
                alert('لطفاً ابتدا محتوای پاسخ را وارد کنید');
                return;
            }
            
            var content = textarea.value;
            var name = nameInput ? nameInput.value : 'پاسخ آماده تست';
            
            var html = content
                .replace(/{نام_مشتری}/g, '<strong style="color: #0d6efd;">احمد محمدی</strong>')
                .replace(/{شماره_تیکت}/g, '<strong style="color: #0dcaf0;">#12345</strong>')
                .replace(/{نام_شرکت}/g, '<strong style="color: #198754;">شرکت نمونه</strong>')
                .replace(/{تاریخ_امروز}/g, '<strong style="color: #ffc107;">' + new Date().toLocaleDateString('fa-IR') + '</strong>')
                .replace(/{نام_کارشناس}/g, '<strong style="color: #6c757d;">علی رضایی</strong>')
                .replace(/\n/g, '<br>');
            
            var win = window.open('', '_blank', 'width=700,height=500');
            if (win) {
                win.document.write('<!DOCTYPE html><html dir="rtl"><head><meta charset="utf-8"><title>تست: ' + name + '</title><link href="https://cdn.jsdelivr.net/npm/bootstrap@5.1.3/dist/css/bootstrap.min.css" rel="stylesheet"></head><body class="p-4"><div class="card"><div class="card-header"><h5>تست پاسخ آماده: ' + name + '</h5></div><div class="card-body">' + html + '</div><div class="card-footer text-center"><button class="btn btn-secondary" onclick="window.close()">بستن</button></div></div></body></html>');
                win.document.close();
            }
        };
        
        window.clearForm = function() {
            console.log('clearForm called');
            if (confirm('آیا می‌خواهید تمام فیلدها پاک شوند؟')) {
                var elements = [
                    document.querySelector('input[name="Name"]'),
                    document.querySelector('textarea[name="Description"]'),
                    document.getElementById('responseContent'),
                    document.getElementById('tagsInput')
                ];
                elements.forEach(function(el) {
                    if (el) el.value = '';
                });
                var checkbox = document.querySelector('input[name="IsActive"]');
                if (checkbox) checkbox.checked = true;
                if (window.updatePreview) window.updatePreview();
            }
        };
        
        window.debugElements = function() {
            console.log('=== CreateCannedResponse DEBUG ===');
            console.log('Elements:');
            console.log('- responseContent:', !!document.getElementById('responseContent'));
            console.log('- previewContent:', !!document.getElementById('previewContent'));
            console.log('- tagsInput:', !!document.getElementById('tagsInput'));
            console.log('Functions:');
            console.log('- insertVariable:', typeof window.insertVariable);
            console.log('- addTag:', typeof window.addTag);
            console.log('- useTemplate:', typeof window.useTemplate);
            console.log('- updatePreview:', typeof window.updatePreview);
            console.log('- testResponse:', typeof window.testResponse);
            console.log('- clearForm:', typeof window.clearForm);
            console.log('=== END DEBUG ===');
            alert('Debug info logged to console (F12)');
        };
    }
    
    // Initialize everything
    function initialize() {
        console.log('CreateCannedResponse: Initializing...');
        
        // Define all global functions
        defineGlobalFunctions();
        
        // Setup auto-update preview
        var textarea = document.getElementById('responseContent');
        if (textarea) {
            textarea.addEventListener('input', window.updatePreview);
            textarea.style.borderColor = '#28a745';
            if (textarea.placeholder && textarea.placeholder.indexOf('(JS Active ✓)') === -1) {
                textarea.placeholder += ' (JS Active ✓)';
            }
        }
        
        // Setup character counter
        if (textarea) {
            var existingCounter = document.getElementById('char-counter');
            if (existingCounter) {
                existingCounter.remove();
            }
            
            var counter = document.createElement('small');
            counter.className = 'text-muted float-end mt-1';
            counter.id = 'char-counter';
            textarea.parentNode.appendChild(counter);
            
            function updateCounter() {
                var count = textarea.value.length;
                counter.textContent = count + ' کاراکتر';
                counter.className = count > 1500 ? 'text-danger float-end mt-1' : 
                                   count > 1000 ? 'text-warning float-end mt-1' : 
                                   'text-muted float-end mt-1';
            }
            textarea.addEventListener('input', updateCounter);
            updateCounter();
        }
        
        // Initial preview
        if (window.updatePreview) {
            window.updatePreview();
        }
        
        console.log('CreateCannedResponse: Initialization complete!');
        
        // Test functions availability
        setTimeout(function() {
            console.log('Function check:');
            console.log('insertVariable:', typeof window.insertVariable);
            console.log('addTag:', typeof window.addTag);
            console.log('useTemplate:', typeof window.useTemplate);
            console.log('updatePreview:', typeof window.updatePreview);
        }, 100);
    }
    
    // Wait for DOM and initialize
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initialize);
    } else {
        initialize();
    }
    
})();