// Ticket Details JavaScript Module
(function() {
    'use strict';
    
    console.log('Loading Ticket Details Module...');
    
    // Global state
    window.allCannedResponses = [];
    
    // Helper functions
    function getAntiForgeryToken() {
        return document.querySelector('input[name="__RequestVerificationToken"]')?.value || '';
    }
    
    function showToast(message, type = 'info') {
        const toast = document.createElement('div');
        toast.className = `alert alert-${type} position-fixed`;
        toast.style.cssText = 'top: 20px; left: 50%; transform: translateX(-50%); z-index: 9999; min-width: 300px;';
        toast.innerHTML = `
            <div class="d-flex align-items-center">
                <i class="bi bi-${type === 'success' ? 'check-circle' : type === 'danger' ? 'exclamation-triangle' : 'info-circle'} me-2"></i>
                ${message}
                <button type="button" class="btn-close ms-auto" onclick="this.parentElement.parentElement.remove()"></button>
            </div>
        `;
        document.body.appendChild(toast);
        
        setTimeout(() => {
            if (toast.parentNode) {
                toast.remove();
            }
        }, 4000);
    }
    
    // Canned Responses Functions
    function loadCannedResponsesData() {
        const loading = document.getElementById('cannedResponsesLoading');
        const list = document.getElementById('cannedResponsesList');
        const empty = document.getElementById('cannedResponsesEmpty');
        
        if (!loading || !list || !empty) {
            console.error('Required modal elements not found');
            return;
        }
        
        loading.style.display = 'block';
        list.style.display = 'none';
        empty.style.display = 'none';
        
        // API call to get canned responses
        fetch('/Knowledge/GetCannedResponses', {
            method: 'GET',
            headers: {
                'Accept': 'application/json',
                'X-Requested-With': 'XMLHttpRequest'
            }
        })
        .then(response => {
            if (!response.ok) {
                throw new Error(`خطای سرور: ${response.status}`);
            }
            return response.json();
        })
        .then(data => {
            loading.style.display = 'none';
            console.log('Loaded canned responses:', data);
            
            if (data && Array.isArray(data) && data.length > 0) {
                window.allCannedResponses = data;
                renderCannedResponses(data);
                list.style.display = 'block';
            } else {
                empty.style.display = 'block';
            }
        })
        .catch(error => {
            console.error('Error loading canned responses:', error);
            loading.style.display = 'none';
            
            // Show error message with retry button
            const errorDiv = document.createElement('div');
            errorDiv.className = 'alert alert-danger text-center';
            errorDiv.innerHTML = `
                <i class="bi bi-exclamation-triangle"></i>
                <strong>خطا در بارگذاری پاسخ‌های آماده</strong>
                <p class="mb-2">${error.message}</p>
                <button class="btn btn-sm btn-outline-danger" onclick="window.loadCannedResponsesData()">
                    <i class="bi bi-arrow-clockwise"></i> تلاش مجدد
                </button>
            `;
            
            list.innerHTML = '';
            list.appendChild(errorDiv);
            list.style.display = 'block';
        });
    }
    
    function renderCannedResponses(responses) {
        const container = document.getElementById('cannedResponsesList');
        if (!container) return;
        
        let html = '';
        
        responses.forEach(response => {
            const shortContent = response.content && response.content.length > 100 ? 
                response.content.substring(0, 100) + '...' : (response.content || '');
            
            const tags = response.tags ? response.tags.split(',').map(tag => 
                `<span class="badge bg-light text-dark border me-1">${tag.trim()}</span>`
            ).join('') : '';
            
            html += `
                <div class="canned-response-item border rounded p-3 mb-3 cursor-pointer hover-effect" 
                     data-response-id="${response.id}" 
                     data-name="${(response.name || '').toLowerCase()}" 
                     data-tags="${response.tags || ''}"
                     onclick="window.selectCannedResponse(${response.id})">
                    <div class="d-flex justify-content-between align-items-start">
                        <div class="flex-grow-1">
                            <h6 class="mb-1 text-primary">${response.name || 'بدون نام'}</h6>
                            ${response.description ? `<p class="text-muted small mb-2">${response.description}</p>` : ''}
                            <div class="response-preview bg-light rounded p-2 mb-2" style="font-size: 0.875em;">
                                ${shortContent.replace(/\n/g, '<br>')}
                            </div>
                            <div class="d-flex justify-content-between align-items-center">
                                <div>${tags}</div>
                                <small class="text-muted">
                                    <i class="bi bi-graph-up"></i> ${response.usageCount || 0} استفاده
                                </small>
                            </div>
                        </div>
                        <div class="ms-3">
                            <button class="btn btn-sm btn-outline-primary" onclick="event.stopPropagation(); window.previewCannedResponse(${response.id})">
                                <i class="bi bi-eye"></i>
                            </button>
                        </div>
                    </div>
                </div>
            `;
        });
        
        container.innerHTML = html;
    }
    
    function selectCannedResponse(responseId) {
        const response = window.allCannedResponses.find(r => r.id === responseId);
        if (!response) {
            showToast('پاسخ آماده یافت نشد', 'danger');
            return;
        }
        
        // Insert into response textarea
        const textarea = document.getElementById('responseContent');
        if (!textarea) {
            showToast('فیلد پاسخ یافت نشد', 'danger');
            return;
        }
        
        const currentContent = textarea.value.trim();
        
        if (currentContent && !confirm('آیا می‌خواهید محتوای فعلی جایگزین شود؟')) {
            return;
        }
        
        textarea.value = response.content || '';
        textarea.focus();
        
        // Close modal
        const modal = bootstrap.Modal.getInstance(document.getElementById('cannedResponsesModal'));
        if (modal) {
            modal.hide();
        }
        
        // Show success message
        showToast(`پاسخ آماده "${response.name}" اعمال شد`, 'success');
        
        // Update usage count
        updateCannedResponseUsage(responseId);
    }
    
    function previewCannedResponse(responseId) {
        const response = window.allCannedResponses.find(r => r.id === responseId);
        if (!response) {
            showToast('پاسخ آماده یافت نشد', 'danger');
            return;
        }
        
        const previewWindow = window.open('', '_blank', 'width=600,height=500');
        if (!previewWindow) {
            showToast('لطفاً popup blocker را غیرفعال کنید', 'warning');
            return;
        }
        
        previewWindow.document.write(`
            <html dir="rtl" lang="fa">
            <head>
                <title>پیش‌نمایش: ${response.name || 'بدون نام'}</title>
                <meta charset="utf-8">
                <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.1.3/dist/css/bootstrap.min.css" rel="stylesheet">
                <link href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.7.2/font/bootstrap-icons.css" rel="stylesheet">
            </head>
            <body class="bg-light">
                <div class="container mt-4">
                    <div class="card">
                        <div class="card-header bg-primary text-white">
                            <h5 class="mb-0">
                                <i class="bi bi-eye"></i> ${response.name || 'بدون نام'}
                            </h5>
                        </div>
                        <div class="card-body">
                            ${response.description ? `<p class="text-muted">${response.description}</p>` : ''}
                            <div class="border rounded p-3 bg-white">
                                ${(response.content || '').replace(/\n/g, '<br>')}
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
    }
    
    function updateCannedResponseUsage(responseId) {
        // Create form data for anti-forgery token
        const formData = new FormData();
        formData.append('id', responseId);
        formData.append('__RequestVerificationToken', getAntiForgeryToken());
        
        // API call to increment usage count
        fetch('/Knowledge/IncrementCannedResponseUsage', {
            method: 'POST',
            headers: {
                'X-Requested-With': 'XMLHttpRequest'
            },
            body: formData
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
    
    function searchCannedResponses() {
        const searchTerm = document.getElementById('cannedResponseSearch')?.value.toLowerCase() || '';
        const category = document.getElementById('cannedResponseCategory')?.value || '';
        
        if (!window.allCannedResponses || !Array.isArray(window.allCannedResponses)) {
            return;
        }
        
        let filteredResponses = window.allCannedResponses.filter(response => {
            const matchesSearch = !searchTerm || 
                (response.name && response.name.toLowerCase().includes(searchTerm)) ||
                (response.content && response.content.toLowerCase().includes(searchTerm)) ||
                (response.description && response.description.toLowerCase().includes(searchTerm));
            
            const matchesCategory = !category || 
                (response.tags && response.tags.toLowerCase().includes(category.toLowerCase()));
            
            return matchesSearch && matchesCategory;
        });
        
        renderCannedResponses(filteredResponses);
    }
    
    // Variable insertion functions
    function insertVariableIntoResponse(variable) {
        const textarea = document.getElementById('responseContent');
        if (!textarea) {
            showToast('فیلد پاسخ یافت نشد', 'danger');
            return;
        }
        
        const start = textarea.selectionStart;
        const end = textarea.selectionEnd;
        const text = textarea.value;
        
        textarea.value = text.substring(0, start) + variable + text.substring(end);
        textarea.focus();
        textarea.setSelectionRange(start + variable.length, start + variable.length);
    }
    
    function closeVariablesModal() {
        const modal = bootstrap.Modal.getInstance(document.getElementById('variablesModal'));
        if (modal) {
            modal.hide();
        }
    }
    
    // Ticket action functions
    function changeTicketStatus(ticketId, newStatus) {
        if (confirm('آیا مطمئن هستید؟')) {
            // Implementation for changing ticket status
            console.log('Changing ticket', ticketId, 'to status', newStatus);
            showToast('این قابلیت در نسخه بعدی اضافه خواهد شد', 'info');
        }
    }
    
    function clearResponse() {
        const textarea = document.getElementById('responseContent');
        if (textarea) {
            textarea.value = '';
            textarea.focus();
        }
    }
    
    function clearNote() {
        const textarea = document.getElementById('noteContent');
        if (textarea) {
            textarea.value = '';
            textarea.focus();
        }
    }
    
    function editResponse(responseId) {
        showToast('قابلیت ویرایش پاسخ در نسخه بعدی اضافه خواهد شد', 'info');
    }
    
    function editNote(noteId) {
        showToast('قابلیت ویرایش یادداشت در نسخه بعدی اضافه خواهد شد', 'info');
    }
    
    // Main functions exposed to global scope
    window.loadCannedResponses = function() {
        const modal = new bootstrap.Modal(document.getElementById('cannedResponsesModal'));
        modal.show();
        loadCannedResponsesData();
    };
    
    window.loadCannedResponsesData = loadCannedResponsesData;
    window.selectCannedResponse = selectCannedResponse;
    window.previewCannedResponse = previewCannedResponse;
    window.searchCannedResponses = searchCannedResponses;
    window.filterCannedResponses = searchCannedResponses; // Alias
    window.insertVariableIntoResponse = insertVariableIntoResponse;
    window.closeVariablesModal = closeVariablesModal;
    
    window.insertVariable = function(variable) {
        if (variable) {
            insertVariableIntoResponse(variable);
        } else {
            const modal = new bootstrap.Modal(document.getElementById('variablesModal'));
            modal.show();
        }
    };
    
    window.changeTicketStatus = changeTicketStatus;
    window.clearResponse = clearResponse;
    window.clearNote = clearNote;
    window.editResponse = editResponse;
    window.editNote = editNote;
    
    // Initialize when DOM is ready
    function initialize() {
        console.log('Ticket Details: Initializing...');
        
        // Setup real-time search for canned responses
        const searchInput = document.getElementById('cannedResponseSearch');
        if (searchInput) {
            searchInput.addEventListener('input', function() {
                clearTimeout(this.searchTimeout);
                this.searchTimeout = setTimeout(() => {
                    searchCannedResponses();
                }, 300);
            });
        }
        
        console.log('Ticket Details: Initialization complete!');
    }
    
    // Wait for DOM and initialize
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initialize);
    } else {
        initialize();
    }
    
})();