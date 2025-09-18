// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Helpio Website JavaScript - Persian/RTL Support

document.addEventListener('DOMContentLoaded', function() {
    // Initialize tooltips
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });

    // Smooth scrolling for anchor links
    document.querySelectorAll('a[href^="#"]').forEach(anchor => {
        anchor.addEventListener('click', function (e) {
            e.preventDefault();
            const target = document.querySelector(this.getAttribute('href'));
            if (target) {
                target.scrollIntoView({
                    behavior: 'smooth',
                    block: 'start'
                });
            }
        });
    });

    // Search functionality for knowledge base and FAQ (Persian support)
    const searchInputs = document.querySelectorAll('input[type="text"][placeholder*="جستجو"], input[type="text"][placeholder*="Search"]');
    searchInputs.forEach(input => {
        input.addEventListener('input', debounce(function() {
            const searchTerm = this.value.toLowerCase().trim();
            const searchableItems = document.querySelectorAll('.searchable-item, .list-group-item, .card-body');
            
            searchableItems.forEach(item => {
                const text = item.textContent.toLowerCase();
                // Support both Persian and English search
                if (text.includes(searchTerm) || searchTerm === '' || searchTerm.length < 2) {
                    item.closest('.card, .list-group-item, .searchable-item').style.display = '';
                } else {
                    item.closest('.card, .list-group-item, .searchable-item').style.display = 'none';
                }
            });
        }, 300));
    });

    // Login type toggle functionality
    const loginTypeRadios = document.querySelectorAll('input[name="loginType"]');
    const passwordField = document.getElementById('passwordField');
    
    if (loginTypeRadios.length > 0 && passwordField) {
        loginTypeRadios.forEach(radio => {
            radio.addEventListener('change', function() {
                if (this.value === 'otp') {
                    passwordField.style.display = 'none';
                } else {
                    passwordField.style.display = 'block';
                }
            });
        });
    }

    // Enhanced signup form with business validation
    const signupForm = document.getElementById('signupForm');
    if (signupForm) {
        signupForm.addEventListener('submit', function(e) {
            e.preventDefault();
            
            const name = document.getElementById('signupName').value.trim();
            const company = document.getElementById('signupCompany').value.trim();
            const email = document.getElementById('signupEmail').value.trim();
            const phone = document.getElementById('signupPhone').value.trim();
            const password = document.getElementById('signupPassword').value.trim();
            const agreeTerms = document.getElementById('agreeTerms').checked;
            
            // Basic validation
            if (!name || !email || !company || !password) {
                showToast('لطفاً تمام فیلدهای ضروری را پر کنید', 'error');
                return;
            }

            if (!agreeTerms) {
                showToast('لطفاً شرایط استفاده را بپذیرید', 'error');
                return;
            }
            
            // Email validation
            const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
            if (!emailPattern.test(email)) {
                showToast('لطفاً ایمیل معتبری وارد کنید', 'error');
                return;
            }

            // Password validation
            if (password.length < 6) {
                showToast('رمز عبور باید حداقل ۶ کاراکتر باشد', 'error');
                return;
            }
            
            // Prepare data for API call
            const formData = {
                name: name,
                company: company,
                email: email,
                phone: phone,
                password: password
            };

            // Show loading state
            const submitButton = document.querySelector('#signupModal button[type="submit"]');
            const originalText = submitButton.innerHTML;
            submitButton.disabled = true;
            submitButton.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>در حال ایجاد حساب...';

            // Call API
            fetch('/Account/Register', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': getAntiForgeryToken()
                },
                body: JSON.stringify(formData)
            })
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    showToast(data.message, 'success');
                    
                    // Track freemium signup event
                    trackEvent('freemium_signup', {
                        company: company,
                        email: email,
                        source: 'website'
                    });
                    
                    // Close modal after 2 seconds and redirect if needed
                    setTimeout(() => {
                        bootstrap.Modal.getInstance(document.getElementById('signupModal')).hide();
                        if (data.redirectUrl) {
                            window.location.href = data.redirectUrl;
                        }
                    }, 2000);
                } else {
                    showToast(data.message || 'خطایی در ثبت‌نام رخ داده است', 'error');
                }
            })
            .catch(error => {
                console.error('Error:', error);
                showToast('خطایی در ثبت‌نام رخ داده است. لطفاً دوباره تلاش کنید.', 'error');
            })
            .finally(() => {
                // Reset button state
                submitButton.disabled = false;
                submitButton.innerHTML = originalText;
            });
        });
    }

    // Enhanced login form
    const loginForm = document.getElementById('loginForm');
    if (loginForm) {
        loginForm.addEventListener('submit', function(e) {
            e.preventDefault();
            
            const emailOrPhone = document.getElementById('loginEmail').value.trim();
            const loginType = document.querySelector('input[name="loginType"]:checked').value;
            const password = document.getElementById('loginPassword').value.trim();
            const rememberMe = document.getElementById('rememberMe').checked;
            
            // Basic validation
            if (!emailOrPhone) {
                showToast('لطفاً ایمیل یا شماره تلفن خود را وارد کنید', 'error');
                return;
            }
            
            if (loginType === 'password' && !password) {
                showToast('لطفاً رمز عبور خود را وارد کنید', 'error');
                return;
            }
            
            // Prepare data for API call
            const formData = {
                emailOrPhone: emailOrPhone,
                loginType: loginType,
                password: password,
                rememberMe: rememberMe
            };

            // Show loading state
            const submitButton = document.querySelector('#loginModal button[type="submit"]');
            const originalText = submitButton.innerHTML;
            submitButton.disabled = true;
            submitButton.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>در حال ورود...';

            // Call API
            fetch('/Account/Login', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': getAntiForgeryToken()
                },
                body: JSON.stringify(formData)
            })
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    if (data.requiresOtp) {
                        showToast(data.message, 'info');
                        showOtpVerification(emailOrPhone);
                    } else {
                        showToast(data.message, 'success');
                        setTimeout(() => {
                            bootstrap.Modal.getInstance(document.getElementById('loginModal')).hide();
                            if (data.redirectUrl) {
                                window.location.href = data.redirectUrl;
                            }
                        }, 1500);
                    }
                } else {
                    showToast(data.message || 'خطایی در ورود رخ داده است', 'error');
                }
            })
            .catch(error => {
                console.error('Error:', error);
                showToast('خطایی در ورود رخ داده است. لطفاً دوباره تلاش کنید.', 'error');
            })
            .finally(() => {
                // Reset button state
                submitButton.disabled = false;
                submitButton.innerHTML = originalText;
            });
        });
    }

    // Live chat functionality (placeholder) - Persian support
    window.startLiveChat = function() {
        // Check if user is logged in
        const isLoggedIn = false; // This would be set from the backend
        
        if (!isLoggedIn) {
            // Show login modal first
            const loginModal = new bootstrap.Modal(document.getElementById('loginModal'));
            loginModal.show();
            return;
        }
        
        // Show chat widget (this would integrate with a real chat service)
        showChatWidget();
    };

    // Chat widget placeholder - Persian
    function showChatWidget() {
        if (document.getElementById('chat-widget')) {
            return; // Already shown
        }

        const chatWidget = document.createElement('div');
        chatWidget.id = 'chat-widget';
        chatWidget.className = 'position-fixed bottom-0 end-0 m-3 bg-primary text-white rounded shadow';
        chatWidget.style.width = '320px';
        chatWidget.style.height = '420px';
        chatWidget.style.zIndex = '1050';
        chatWidget.dir = 'rtl';
        
        chatWidget.innerHTML = `
            <div class="d-flex justify-content-between align-items-center p-3 border-bottom">
                <h6 class="mb-0">گفتگوی آنلاین</h6>
                <button type="button" class="btn-close btn-close-white" onclick="closeChatWidget()"></button>
            </div>
            <div class="p-3" style="height: 320px; overflow-y: auto;">
                <div class="mb-3">
                    <div class="bg-light text-dark p-2 rounded">
                        <small><strong>کارشناس پشتیبانی:</strong> سلام! چطور می‌تونم کمکتون کنم؟</small>
                    </div>
                </div>
            </div>
            <div class="p-3 border-top">
                <div class="input-group">
                    <input type="text" class="form-control" placeholder="پیام خود را بنویسید..." dir="rtl">
                    <button class="btn btn-light" type="button">
                        <i class="fas fa-paper-plane"></i>
                    </button>
                </div>
            </div>
        `;
        
        document.body.appendChild(chatWidget);
    }

    // Close chat widget
    window.closeChatWidget = function() {
        const chatWidget = document.getElementById('chat-widget');
        if (chatWidget) {
            chatWidget.remove();
        }
    };

    // Enhanced form validation with Persian messages
    const forms = document.querySelectorAll('form[data-needs-validation]');
    forms.forEach(form => {
        form.addEventListener('submit', function(event) {
            if (!form.checkValidity()) {
                event.preventDefault();
                event.stopPropagation();
                showToast('لطفاً تمام فیلدهای ضروری را به درستی پر کنید', 'error');
            }
            form.classList.add('was-validated');
        });
    });

    // Navbar scroll effect
    const navbar = document.querySelector('.navbar');
    if (navbar) {
        window.addEventListener('scroll', function() {
            if (window.scrollY > 50) {
                navbar.classList.add('navbar-scrolled');
            } else {
                navbar.classList.remove('navbar-scrolled');
            }
        });
    }

    // Auto-hide alerts after 5 seconds
    const alerts = document.querySelectorAll('.alert:not(.alert-permanent)');
    alerts.forEach(alert => {
        setTimeout(() => {
            const bsAlert = new bootstrap.Alert(alert);
            bsAlert.close();
        }, 5000);
    });

    // FAQ accordion search - Persian support
    const faqSearch = document.getElementById('faqSearch');
    if (faqSearch) {
        faqSearch.addEventListener('input', function() {
            const searchTerm = this.value.toLowerCase().trim();
            const faqItems = document.querySelectorAll('.accordion-item');
            
            faqItems.forEach(item => {
                const text = item.textContent.toLowerCase();
                if (text.includes(searchTerm) || searchTerm === '' || searchTerm.length < 2) {
                    item.style.display = 'block';
                } else {
                    item.style.display = 'none';
                }
            });
        });
    }

    // Feature cards hover effect
    const featureCards = document.querySelectorAll('.card');
    featureCards.forEach(card => {
        card.addEventListener('mouseenter', function() {
            this.style.transform = 'translateY(-5px)';
        });
        
        card.addEventListener('mouseleave', function() {
            this.style.transform = 'translateY(0)';
        });
    });

    // Loading states for forms with Persian messages
    const submitButtons = document.querySelectorAll('button[type="submit"]');
    submitButtons.forEach(button => {
        // Store original text
        button.dataset.originalText = button.innerHTML;
        
        button.addEventListener('click', function() {
            const form = this.closest('form');
            if (form && form.checkValidity()) {
                this.disabled = true;
                
                // Check button text to show appropriate loading message
                if (button.textContent.includes('ورود')) {
                    this.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>در حال ورود...';
                } else if (button.textContent.includes('شروع') || button.textContent.includes('ثبت‌نام')) {
                    this.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>در حال ایجاد حساب...';
                } else if (button.textContent.includes('ارسال')) {
                    this.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>در حال ارسال...';
                } else {
                    this.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>در حال پردازش...';
                }
                
                // Re-enable after 3 seconds (in real app, this would be handled by form submission)
                setTimeout(() => {
                    this.disabled = false;
                    this.innerHTML = this.dataset.originalText;
                }, 3000);
            }
        });
    });

    // Contact form enhancement for support
    const contactForms = document.querySelectorAll('form[data-contact-form]');
    contactForms.forEach(form => {
        form.addEventListener('submit', function(e) {
            e.preventDefault();
            showToast('درخواست شما با موفقیت ارسال شد. تیم ما در اسرع وقت با شما تماس خواهد گرفت.', 'success');
            
            // Track contact form submission
            trackEvent('contact_form_submit', {
                form_type: this.dataset.contactForm || 'general',
                page: window.location.pathname
            });
        });
    });
});

// OTP Verification Modal
function showOtpVerification(emailOrPhone) {
    // Hide login modal
    bootstrap.Modal.getInstance(document.getElementById('loginModal')).hide();
    
    // Create OTP verification modal
    const otpModal = document.createElement('div');
    otpModal.className = 'modal fade';
    otpModal.id = 'otpModal';
    otpModal.setAttribute('tabindex', '-1');
    otpModal.dir = 'rtl';
    
    otpModal.innerHTML = `
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title">تأیید کد یکبار مصرف</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                </div>
                <div class="modal-body">
                    <p>کد تأیید به <strong>${emailOrPhone}</strong> ارسال شد.</p>
                    <form id="otpForm">
                        <div class="mb-3">
                            <label for="otpCode" class="form-label">کد تأیید</label>
                            <input type="text" class="form-control text-center" id="otpCode" placeholder="کد ۴-۶ رقمی را وارد کنید" maxlength="6">
                        </div>
                    </form>
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">بستن</button>
                    <button type="button" class="btn btn-primary" onclick="verifyOtp('${emailOrPhone}')">تأیید</button>
                </div>
            </div>
        </div>
    `;
    
    document.body.appendChild(otpModal);
    const modal = new bootstrap.Modal(otpModal);
    modal.show();
    
    // Clean up when modal is hidden
    otpModal.addEventListener('hidden.bs.modal', function() {
        otpModal.remove();
    });
}

// Verify OTP
function verifyOtp(emailOrPhone) {
    const otpCode = document.getElementById('otpCode').value.trim();
    
    if (!otpCode || otpCode.length < 4) {
        showToast('لطفاً کد تأیید را وارد کنید', 'error');
        return;
    }
    
    const formData = {
        emailOrPhone: emailOrPhone,
        otpCode: otpCode
    };

    // Show loading state
    const submitButton = document.querySelector('#otpModal .btn-primary');
    const originalText = submitButton.innerHTML;
    submitButton.disabled = true;
    submitButton.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>در حال تأیید...';

    fetch('/Account/VerifyOtp', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': getAntiForgeryToken()
        },
        body: JSON.stringify(formData)
    })
    .then(response => response.json())
    .then(data => {
        if (data.success) {
            showToast(data.message, 'success');
            setTimeout(() => {
                bootstrap.Modal.getInstance(document.getElementById('otpModal')).hide();
                if (data.redirectUrl) {
                    window.location.href = data.redirectUrl;
                }
            }, 1500);
        } else {
            showToast(data.message || 'کد تأیید نامعتبر است', 'error');
        }
    })
    .catch(error => {
        console.error('Error:', error);
        showToast('خطایی در تأیید کد رخ داده است. لطفاً دوباره تلاش کنید.', 'error');
    })
    .finally(() => {
        // Reset button state
        submitButton.disabled = false;
        submitButton.innerHTML = originalText;
    });
}

// Get anti-forgery token
function getAntiForgeryToken() {
    // This should be implemented based on your anti-forgery token setup
    // For now, return empty string
    return '';
}

// Utility functions
function debounce(func, wait) {
    let timeout;
    return function executedFunction(...args) {
        const later = () => {
            clearTimeout(timeout);
            func(...args);
        };
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
    };
}

// Enhanced toast function with Persian support
function showToast(message, type = 'info') {
    const toast = document.createElement('div');
    toast.className = `toast position-fixed top-0 end-0 m-3`;
    toast.setAttribute('role', 'alert');
    toast.setAttribute('aria-live', 'assertive');
    toast.setAttribute('aria-atomic', 'true');
    toast.dir = 'rtl';
    
    let bgClass = 'bg-info';
    let icon = 'fas fa-info-circle';
    
    switch(type) {
        case 'success':
            bgClass = 'bg-success';
            icon = 'fas fa-check-circle';
            break;
        case 'error':
            bgClass = 'bg-danger';
            icon = 'fas fa-exclamation-circle';
            break;
        case 'warning':
            bgClass = 'bg-warning';
            icon = 'fas fa-exclamation-triangle';
            break;
    }
    
    toast.innerHTML = `
        <div class="toast-body ${bgClass} text-white d-flex align-items-center">
            <i class="${icon} me-2"></i>
            <span>${message}</span>
            <button type="button" class="btn-close btn-close-white ms-auto" data-bs-dismiss="toast"></button>
        </div>
    `;
    
    document.body.appendChild(toast);
    
    const bsToast = new bootstrap.Toast(toast, {
        autohide: true,
        delay: 5000
    });
    bsToast.show();
    
    toast.addEventListener('hidden.bs.toast', function() {
        toast.remove();
    });
}

// Copy to clipboard function with Persian message
function copyToClipboard(text) {
    navigator.clipboard.writeText(text).then(function() {
        showToast('با موفقیت کپی شد!', 'success');
    }).catch(function(err) {
        console.error('Failed to copy text: ', err);
        showToast('خطا در کپی کردن', 'error');
    });
}

// Analytics tracking for business goals
function trackEvent(eventName, properties = {}) {
    // This would integrate with your analytics service
    console.log('Event tracked:', eventName, properties);
    
    // Track key business goal events
    if (eventName === 'freemium_signup') {
        console.log('Freemium signup tracked - contributing to 1000 user goal');
    }
    
    if (eventName === 'upgrade_to_growth') {
        console.log('Growth plan upgrade tracked - contributing to 10% conversion goal');
    }
}

// Page view tracking
window.addEventListener('load', function() {
    trackEvent('page_view', {
        page: window.location.pathname,
        title: document.title,
        language: 'fa'
    });
});

// Persian number conversion utility
function toPersianNumber(num) {
    const persianDigits = '۰۱۲۳۴۵۶۷۸۹';
    return num.toString().replace(/\d/g, (digit) => persianDigits[digit]);
}

// Format Persian currency
function formatPersianCurrency(amount) {
    return new Intl.NumberFormat('fa-IR', {
        style: 'currency',
        currency: 'IRR',
        minimumFractionDigits: 0
    }).format(amount);
}

// Initialize Persian number display
document.addEventListener('DOMContentLoaded', function() {
    // Convert numbers in specific elements to Persian
    const persianNumberElements = document.querySelectorAll('.persian-numbers, .display-4, .display-5, .display-6');
    persianNumberElements.forEach(element => {
        if (element.textContent.match(/\d/)) {
            const persianText = element.textContent.replace(/\d+/g, (match) => toPersianNumber(match));
            element.textContent = persianText;
        }
    });
});
