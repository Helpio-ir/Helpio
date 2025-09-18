/**
 * Ticket Details Page JavaScript
 * مدیریت صفحه جزئیات تیکت
 */

// Global variables
window.allCannedResponses = [];

/**
 * تغییر وضعیت تیکت
 */
function changeTicketStatus(ticketId, newStatus) {
    if (confirm('آیا مطمئن هستید؟')) {
        // Implementation for changing ticket status
        console.log('Changing ticket', ticketId, 'to status', newStatus);
        // You can implement AJAX call here
    }
}

/**
 * پاک کردن محتوای پاسخ
 */
function clearResponse() {
    const textarea = document.getElementById('responseContent');
    if (textarea) {
        textarea.value = '';
        textarea.focus();
    }
}

/**
 * پاک کردن محتوای یادداشت
 */
function clearNote() {
    const textarea = document.getElementById('noteContent');
    if (textarea) {
        textarea.value = '';
        textarea.focus();
    }
}

/**
 * ویرایش پاسخ
 */
function editResponse(responseId) {
    // Implementation for editing response
    alert('قابلیت ویرایش پاسخ در نسخه بعدی اضافه خواهد شد');
}

/**
 * ویرایش یادداشت
 */
function editNote(noteId) {
    // Implementation for editing note
    alert('قابلیت ویرایش یادداشت در نسخه بعدی اضافه خواهد شد');
}

/**
 * نمایش modal پاسخ‌های آماده
 */
function loadCannedResponses() {
    try {
        const modal = new bootstrap.Modal(document.getElementById('cannedResponsesModal'));
        modal.show();
        loadCannedResponsesData();
    } catch (error) {
        console.error('Error showing canned responses modal:', error);
        showToast('خطا در نمایش پاسخ‌های آماده', 'danger');
    }
}

/**
 * بارگذاری داده‌های پاسخ‌های آماده از API
 */
function loadCannedResponsesData() {
    const loading = document.getElementById('cannedResponsesLoading');
    const list = document.getElementById('cannedResponsesList');
    const empty = document.getElementById('cannedResponsesEmpty');
    
    if (!loading || !list || !empty) {
        console.error('Required DOM elements not found for canned responses');
        return;
    }
    
    // Show loading state
    loading.style.display = 'block';
    list.style.display = 'none';
    empty.style.display = 'none';
    
    // Get the API URL (will be replaced by server-side rendering)
    const url = window.cannedResponsesApiUrl || '/Knowledge/GetCannedResponses';
    console.log('Calling canned responses API:', url);
    
    fetch(url, {
        method: 'GET',
        headers: {
            'Accept': 'application/json',
            'X-Requested-With': 'XMLHttpRequest'
        }
    })
    .then(response => {
        console.log('Response status:', response.status);
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }
        return response.json();
    })
    .then(data => {
        console.log('Received canned responses data:', data);
        loading.style.display = 'none';
        
        if (data && data.length > 0) {
            renderCannedResponses(data);
            list.style.display = 'block';
        } else {
            console.log('No canned responses found');
            empty.style.display = 'block';
        }
    })
    .catch(error => {
        console.error('Error loading canned responses:', error);
        loading.style.display = 'none';
        empty.style.display = 'block';
        
        // Show error message to user
        showToast(`خطا در بارگذاری پاسخ‌های آماده: ${error.message}`, 'danger');
        
        // Fallback - show sample data for testing
        const sampleData = [
            {
                id: 1,
                name: 'سلام و احوالپرسی',
                content: 'سلام {نام_مشتری} عزیز،\n\nامیدوارم حال شما خوب باشد. با تشکر از تماس شما با تیم پشتیبانی ما.',
                description: 'پاسخ استاندارد برای شروع مکالمه',
                tags: 'پشتیبانی,عمومی',
                usageCount: 25
            },
            {
                id: 2,
                name: 'درخواست اطلاعات تکمیلی',
                content: 'سلام {نام_مشتری} عزیز،\n\nبرای بررسی دقیق‌تر مشکل شما، لطفاً اطلاعات زیر را ارسال کنید:\n\n- توضیح کامل مشکل\n- تصاویر مرتبط (در صورت وجود)\n- زمان وقوع مشکل',
                description: 'درخواست اطلاعات بیشتر از مشتری',
                tags: 'پشتیبانی,بررسی',
                usageCount: 18
            },
            {
                id: 3,
                name: 'تشکر و خداحافظی',
                content: 'در صورت داشتن سوال اضافی، لطفاً با ما در تماس باشید.\n\nبا تشکر از صبر و شکیبایی شما،\n{نام_کارشناس}\nتیم پشتیبانی',
                description: 'پایان دادن به مکالمه با تشکر',
                tags: 'عمومی,تشکر',
                usageCount: 32
            },
            {
                id: 4,
                name: 'راهنمای ورود به سیستم',
                content: 'برای ورود به سیستم لطفاً مراحل زیر را دنبال کنید:\n\n1. به آدرس سایت مراجعه کنید\n2. نام کاربری و رمز عبور خود را وارد کنید\n3. روی دکمه ورود کلیک کنید\n\nدر صورت فراموشی رمز عبور، از گزینه "رمز عبور را فراموش کرده‌ام" استفاده کنید.',
                description: 'راهنمای ورود برای کاربران',
                tags: 'فنی,ورود,راهنمایی',
                usageCount: 15
            },
            {
                id: 5,
                name: 'مشکل فنی در حال بررسی',
                content: 'سلام {نام_مشتری} عزیز،\n\nمشکل فنی گزارش شده توسط شما در دست بررسی تیم فنی ما قرار گرفته است. به محض حل مشکل، نتیجه را با شما در میان خواهیم گذاشت.\n\nزمان تخمینی حل مشکل: 24 ساعت کاری\n\nبا تشکر از صبر شما',
                description: 'اطلاع‌رسانی در مورد بررسی مشکل فنی',
                tags: 'فنی,بررسی,انتظار',
                usageCount: 12
            }
        ];
        
        console.log('Using fallback sample data');
        renderCannedResponses(sampleData);
        list.style.display = 'block';
        empty.style.display = 'none';
    });
}

/**
 * رندر کردن لیست پاسخ‌های آماده
 */
function renderCannedResponses(responses) {
    const container = document.getElementById('cannedResponsesList');
    if (!container) {
        console.error('Canned responses container not found');
        return;
    }
    
    let html = '';
    
    responses.forEach(response => {
        const shortContent = response.content && response.content.length > 100 ? 
            response.content.substring(0, 100) + '...' : (response.content || '');
        
        const tags = response.tags ? response.tags.split(',').map(tag => 
            `<span class="badge bg-light text-dark border me-1">${escapeHtml(tag.trim())}</span>`
        ).join('') : '';
        
        html += `
            <div class="canned-response-item border rounded p-3 mb-3 cursor-pointer hover-effect" 
                 data-response-id="${response.id}" 
                 data-name="${escapeHtml(response.name.toLowerCase())}" 
                 data-tags="${escapeHtml(response.tags || '')}"
                 onclick="selectCannedResponse(${response.id})">
                <div class="d-flex justify-content-between align-items-start">
                    <div class="flex-grow-1">
                        <h6 class="mb-1 text-primary">${escapeHtml(response.name)}</h6>
                        ${response.description ? `<p class="text-muted small mb-2">${escapeHtml(response.description)}</p>` : ''}
                        <div class="response-preview bg-light rounded p-2 mb-2" style="font-size: 0.875em;">
                            ${escapeHtml(shortContent).replace(/\n/g, '<br>')}
                        </div>
                        <div class="d-flex justify-content-between align-items-center">
                            <div>${tags}</div>
                            <small class="text-muted">
                                <i class="bi bi-graph-up"></i> ${response.usageCount || 0} استفاده
                            </small>
                        </div>
                    </div>
                    <div class="ms-3">
                        <button class="btn btn-sm btn-outline-primary" onclick="event.stopPropagation(); previewCannedResponse(${response.id})">
                            <i class="bi bi-eye"></i>
                        </button>
                    </div>
                </div>
            </div>
        `;
    });
    
    container.innerHTML = html;
    
    // Store responses data for search and filtering
    window.allCannedResponses = responses;
}

/**
 * انتخاب و اعمال پاسخ آماده
 */
function selectCannedResponse(responseId) {
    const response = window.allCannedResponses.find(r => r.id === responseId);
    if (!response) {
        console.error('Canned response not found:', responseId);
        showToast('پاسخ آماده یافت نشد', 'danger');
        return;
    }
    
    // Insert into response textarea
    const textarea = document.getElementById('responseContent');
    if (!textarea) {
        console.error('Response textarea not found');
        showToast('فیلد پاسخ یافت نشد', 'danger');
        return;
    }
    
    const currentContent = textarea.value.trim();
    
    if (currentContent && !confirm('آیا می‌خواهید محتوای فعلی جایگزین شود؟')) {
        return;
    }
    
    textarea.value = response.content;
    textarea.focus();
    
    // Close modal
    try {
        const modal = bootstrap.Modal.getInstance(document.getElementById('cannedResponsesModal'));
        if (modal) {
            modal.hide();
        }
    } catch (error) {
        console.error('Error closing modal:', error);
    }
    
    // Show success message
    showToast(`پاسخ آماده "${response.name}" اعمال شد`, 'success');
    
    // Update usage count
    updateCannedResponseUsage(responseId);
}

/**
 * پیش‌نمایش پاسخ آماده
 */
function previewCannedResponse(responseId) {
    const response = window.allCannedResponses.find(r => r.id === responseId);
    if (!response) {
        console.error('Canned response not found for preview:', responseId);
        return;
    }
    
    try {
        const previewWindow = window.open('', '_blank', 'width=700,height=600');
        if (!previewWindow) {
            alert('لطفاً popup blocker را غیرفعال کنید');
            return;
        }
        
        // Process content with variables replaced
        const processedContent = replaceVariablesForPreview(response.content);
        
        previewWindow.document.write(`
            <html dir="rtl" lang="fa">
            <head>
                <title>پیش‌نمایش: ${escapeHtml(response.name)}</title>
                <meta charset="utf-8">
                <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.1.3/dist/css/bootstrap.min.css" rel="stylesheet">
                <link href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.7.2/font/bootstrap-icons.css" rel="stylesheet">
                <style>
                    .variable-highlight {
                        background-color: #e3f2fd;
                        border: 1px solid #2196f3;
                        border-radius: 4px;
                        padding: 2px 4px;
                        margin: 0 2px;
                    }
                </style>
            </head>
            <body class="bg-light">
                <div class="container mt-4">
                    <div class="card">
                        <div class="card-header bg-primary text-white">
                            <h5 class="mb-0">
                                <i class="bi bi-eye"></i> ${escapeHtml(response.name)}
                            </h5>
                        </div>
                        <div class="card-body">
                            <div class="mb-3">
                                ${response.description ? `<p class="text-muted">${escapeHtml(response.description)}</p>` : ''}
                            </div>
                            
                            <div class="alert alert-info mb-3">
                                <i class="bi bi-info-circle"></i>
                                <strong>توجه:</strong> متغیرهای زیر با مقادیر واقعی جایگزین خواهند شد:
                            </div>
                            
                            <div class="border rounded p-3 bg-white">
                                <h6 class="text-muted mb-3">پیش‌نمایش با داده‌های نمونه:</h6>
                                <div style="line-height: 1.8;">${processedContent.replace(/\n/g, '<br>')}</div>
                            </div>
                            
                            <div class="mt-3">
                                <h6 class="text-muted">محتوای اصلی (خام):</h6>
                                <div class="border rounded p-3 bg-light">
                                    <code>${escapeHtml(response.content).replace(/\n/g, '<br>')}</code>
                                </div>
                            </div>
                            
                            <div class="mt-3 text-center">
                                <button class="btn btn-secondary" onclick="window.close()">بستن</button>
                            </div>
                        </div>
                    </div>
                </div>
            </body>
            </html>
        `);
        previewWindow.document.close();
    } catch (error) {
        console.error('Error opening preview window:', error);
        showToast('خطا در نمایش پیش‌نمایش', 'danger');
    }
}

/**
 * جستجو در پاسخ‌های آماده
 */
function searchCannedResponses() {
    const searchInput = document.getElementById('cannedResponseSearch');
    const categorySelect = document.getElementById('cannedResponseCategory');
    
    if (!searchInput || !categorySelect) {
        console.error('Search elements not found');
        return;
    }
    
    const searchTerm = searchInput.value.toLowerCase();
    const category = categorySelect.value;
    
    if (!window.allCannedResponses) {
        console.warn('No canned responses data available for search');
        return;
    }
    
    let filteredResponses = window.allCannedResponses.filter(response => {
        const matchesSearch = !searchTerm || 
            response.name.toLowerCase().includes(searchTerm) ||
            response.content.toLowerCase().includes(searchTerm) ||
            (response.description && response.description.toLowerCase().includes(searchTerm));
        
        const matchesCategory = !category || 
            (response.tags && response.tags.toLowerCase().includes(category.toLowerCase()));
        
        return matchesSearch && matchesCategory;
    });
    
    renderCannedResponses(filteredResponses);
}

/**
 * فیلتر کردن پاسخ‌های آماده بر اساس دسته‌بندی
 */
function filterCannedResponses() {
    searchCannedResponses(); // Reuse search function
}

/**
 * به‌روزرسانی تعداد استفاده از پاسخ آماده
 */
function updateCannedResponseUsage(responseId) {
    const url = (window.incrementUsageApiUrl || '/Knowledge/IncrementCannedResponseUsage') + '?id=' + responseId;
    
    fetch(url, {
        method: 'POST',
        headers: {
            'X-Requested-With': 'XMLHttpRequest'
        }
    })
    .then(response => response.json())
    .then(data => {
        if (data.success) {
            console.log('Usage count updated for response:', responseId, 'New count:', data.newUsageCount);
        } else {
            console.warn('Failed to update usage count:', data.message);
        }
    })
    .catch(error => {
        console.error('Error updating usage count:', error);
    });
}

/**
 * وارد کردن متغیر به textarea پاسخ
 */
function insertVariableIntoResponse(variable) {
    const textarea = document.getElementById('responseContent');
    if (!textarea) {
        console.error('Response textarea not found');
        return;
    }
    
    const start = textarea.selectionStart;
    const end = textarea.selectionEnd;
    const text = textarea.value;
    
    textarea.value = text.substring(0, start) + variable + text.substring(end);
    textarea.focus();
    textarea.setSelectionRange(start + variable.length, start + variable.length);
}

/**
 * بستن modal متغیرها
 */
function closeVariablesModal() {
    try {
        const modal = bootstrap.Modal.getInstance(document.getElementById('variablesModal'));
        if (modal) {
            modal.hide();
        }
    } catch (error) {
        console.error('Error closing variables modal:', error);
    }
}

/**
 * نمایش toast notification
 */
function showToast(message, type = 'info') {
    try {
        const toast = document.createElement('div');
        toast.className = `alert alert-${type} position-fixed`;
        toast.style.cssText = 'top: 20px; left: 50%; transform: translateX(-50%); z-index: 9999; min-width: 300px;';
        toast.innerHTML = `
            <div class="d-flex align-items-center">
                <i class="bi bi-${type === 'success' ? 'check-circle' : type === 'danger' ? 'exclamation-triangle' : 'info-circle'} me-2"></i>
                ${escapeHtml(message)}
                <button type="button" class="btn-close ms-auto" onclick="this.parentElement.parentElement.remove()"></button>
            </div>
        `;
        document.body.appendChild(toast);
        
        setTimeout(() => {
            if (toast.parentNode) {
                toast.remove();
            }
        }, 4000);
    } catch (error) {
        console.error('Error showing toast:', error);
        // Fallback to alert
        alert(message);
    }
}

/**
 * نمایش modal متغیرها یا وارد کردن متغیر مشخص
 */
function insertVariable(variable) {
    if (variable) {
        insertVariableIntoResponse(variable);
    } else {
        try {
            const modal = new bootstrap.Modal(document.getElementById('variablesModal'));
            modal.show();
        } catch (error) {
            console.error('Error showing variables modal:', error);
            showToast('خطا در نمایش متغیرها', 'danger');
        }
    }
}

/**
 * Escape HTML to prevent XSS
 */
function escapeHtml(text) {
    if (!text) return '';
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}

/**
 * Initialize page when DOM is loaded
 */
document.addEventListener('DOMContentLoaded', function() {
    console.log('Ticket Details page initialized');
    
    // Real-time search for canned responses
    const searchInput = document.getElementById('cannedResponseSearch');
    if (searchInput) {
        searchInput.addEventListener('input', function() {
            clearTimeout(this.searchTimeout);
            this.searchTimeout = setTimeout(() => {
                searchCannedResponses();
            }, 300);
        });
    }
    
    // Initialize any other components if needed
    initializeOtherComponents();
});

/**
 * Initialize other page components
 */
function initializeOtherComponents() {
    // Add any additional initialization code here
    console.log('Other components initialized');
}

/**
 * جایگزینی متغیرها برای پیش‌نمایش
 */
function replaceVariablesForPreview(content) {
    if (!content) return content;
    
    // نمونه داده‌ها برای پیش‌نمایش
    const sampleData = {
        '{نام_مشتری}': 'احمد محمدی',
        '{شماره_تیکت}': window.currentTicketId || '234',
        '{نام_شرکت}': 'شرکت نمونه تجاری',
        '{تاریخ_امروز}': new Date().toLocaleDateString('fa-IR'),
        '{نام_کارشناس}': 'علی رضایی',
        '{ایمیل_مشتری}': 'customer@example.com',
        '{تلفن_مشتری}': '09123456789',
        '{عنوان_تیکت}': 'مشکل فنی',
        '{دسته_بندی_تیکت}': 'Technical Issues',
        '{زمان_فعلی}': new Date().toLocaleTimeString('fa-IR', { hour: '2-digit', minute: '2-digit' }),
        '{نام_سایت}': 'Helpio',
        '{آدرس_سایت}': 'https://helpio.io'
    };
    
    let result = content;
    Object.entries(sampleData).forEach(([variable, value]) => {
        const regex = new RegExp(escapeRegExp(variable), 'g');
        result = result.replace(regex, `<span class="text-primary fw-bold">${value}</span>`);
    });
    
    return result;
}

/**
 * Escape special characters for regex
 */
function escapeRegExp(string) {
    return string.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
}