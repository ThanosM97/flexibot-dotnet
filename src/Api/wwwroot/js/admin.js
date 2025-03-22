document.addEventListener('DOMContentLoaded', () => {
    // DOM Elements
    const docDropZone = document.getElementById('docDropZone');
    const docInput = document.getElementById('docInput');
    const docStatus = document.getElementById('docStatus');
    const qnaInput = document.getElementById('qnaInput');
    const documentList = document.getElementById('documentList');

    // Initialize Page
    loadDocumentList();
    loadQnAFile();

    // Document Upload Handling
    docDropZone.addEventListener('click', () => docInput.click());
    docDropZone.addEventListener('drop', async (e) => {
        e.preventDefault();
        await handleFileUpload(e.dataTransfer.files, '/documents/upload', docStatus);
    });
    docInput.addEventListener('change', async (e) => {
        await handleFileUpload(e.target.files, '/documents/upload', docStatus);
    });

    // Drag and Drop Events
    ['dragover', 'dragleave', 'drop'].forEach(event => {
        docDropZone.addEventListener(event, (e) => {
            e.preventDefault();
            docDropZone.classList.toggle('dragover', e.type === 'dragover');
        });
    });

    // QnA File Upload Handling
    qnaInput.addEventListener('change', async (e) => {
        const file = e.target.files[0];
        if (!file) return;

        const formData = new FormData();
        formData.append('file', file);

        try {
            const response = await fetch('/qna/upload', { method: 'POST', body: formData });
            if (response.ok) await loadQnAFile();
        } catch (error) {
            console.log("QnA upload error.")
        } finally {
            qnaInput.value = ''; // Reset input
        }
    });

    // Function to Handle Document Upload
    async function handleFileUpload(files, endpoint) {
        if (files.length === 0) return;

        const formData = new FormData();
        Array.from(files).forEach(file => formData.append('files', file));

        try {
            const response = await fetch(endpoint, { method: 'POST', body: formData });
            const result = await response.json();

            if (response.ok) {
                result.jobIds.forEach((jobId, index) => {
                    const file = files[index];
                    if (!file) return;
                    const listItem = createUploadingItem(file);
                    documentList.prepend(listItem);
                    trackDocumentStatus(listItem, jobId);
                });
            }
        } catch (error) {
            console.log("Upload failed.")
        }
    }

    // Function to Load Document List
    async function loadDocumentList() {
        try {
            const response = await fetch('/documents/list');
            const documents = await response.json();
            documentList.innerHTML = documents.map(doc => `
                <div class="document-item">
                    <i class="${getFileIcon(doc.extension)} document-icon"></i>
                    <div class="document-info">
                        <div class="document-name">${doc.fileName}</div>
                        <div class="document-meta">${formatKBytes(doc.size)}</div>
                    </div>
                    <div class="document-actions">
                        <button class="action-button download-button" onclick="downloadDocument('${doc.documentId}')">
                            <i class="fas fa-download"></i>
                        </button>
                        <button class="action-button delete-button" onclick="deleteDocument('${doc.documentId}')">
                            <i class="fas fa-trash"></i>
                        </button>
                    </div>
                </div>`
            ).join('');
        } catch (error) {
            console.error('Failed to load documents:', error);
        }
    }

    // Function to Load QnA File
    async function loadQnAFile() {
        try {
            const response = await fetch('/qna/download');
            qnaList.innerHTML = response.status === 200 ? `
                <div class="qna-item">
                    <i class="fas fa-file-csv qna-icon"></i>
                    <div class="document-info">
                        <div class="document-name">qna_knowledge_base.csv</div>
                        <div class="document-meta">QnA Knowledge Base</div>
                    </div>
                    <div class="document-actions">
                        <button class="action-button download-button" onclick="downloadQnA()">
                            <i class="fas fa-download"></i>
                        </button>
                        <button class="action-button delete-button" onclick="deleteQnA()">
                            <i class="fas fa-trash"></i>
                        </button>
                    </div>
                </div>` : '<div class="upload-status">No QnA file uploaded</div>';
        } catch (error) {
            qnaList.innerHTML = '<div class="upload-status error">Error loading QnA status</div>';
        }
    }

    // Utility Functions
    function createUploadingItem(file) {
        const item = document.createElement('div');
        item.className = 'document-item processing-item';
        item.innerHTML = `
            <i class="${getFileIcon(file.name.split('.').pop())} document-icon"></i>
            <div class="document-info">
                <div class="document-name">${file.name}</div>
                <div class="document-meta">${formatKBytes(file.size)}</div>
            </div>
            <div class="status-indicator">
                <div class="status-spinner"></div>
                <span>Processing...</span>
            </div>`;
        return item;
    }

    async function trackDocumentStatus(listItem, jobId) {
        const checkStatus = async () => {
            try {
                const response = await fetch(`/documents/status/${jobId}`);
                const progress = await response.json();

                switch (progress.status) {
                    case 'Completed':
                        listItem.classList.remove('processing-item');
                        listItem.querySelector('.status-indicator').innerHTML = `
                            <i class="fas fa-check-circle" style="color: #4CAF50;"></i>
                            <span>Processed</span>
                        `;
                        loadDocumentList(); // Refresh full list
                        break;

                    case 'Failed':
                        setItemError(listItem, progress.message || 'Processing failed');
                        break;

                    case 'Processing':
                        setTimeout(checkStatus, 3000);
                        break;

                    case 'Deleted':
                        listItem.remove();
                        break;

                    default:
                        setItemError(listItem, 'Processing failed');
                        break;
                }
            } catch (error) {
                setItemError(listItem, 'Status check failed');
            }
        };

        checkStatus();
    }

    function setItemError(listItem, message) {
        listItem.classList.remove('processing-item');
        listItem.querySelector('.status-indicator').innerHTML = `
            <i class="fas fa-times-circle" style="color: #f44336;"></i>
            <span>${message}</span>
        `;
    }

    function getFileIcon(extension) {
        const icons = { pdf: 'fas fa-file-pdf', doc: 'fas fa-file-word', docx: 'fas fa-file-word', txt: 'fas fa-file-alt' };
        return icons[extension.slice(1).toLowerCase()] || 'fas fa-file';
    }

    function formatKBytes(kbytes) {
        if (kbytes === 0) return '0 KB';
        const k = 1024;
        const sizes = ['KB', 'MB', 'GB'];
        const i = Math.floor(Math.log(kbytes) / Math.log(k));
        return parseFloat((kbytes / Math.pow(k, i)).toFixed(1)) + ' ' + sizes[i];
    }

    // Document Actions
    window.downloadDocument = async (documentId) => window.open(`/documents/download/${documentId}`, '_blank');
    window.deleteDocument = async (documentId) => confirm('Are you sure you want to delete this document?') && (await fetch(`/documents/delete/${documentId}`, { method: 'DELETE' })).ok && loadDocumentList();
    window.downloadQnA = () => window.open('/qna/download', '_blank');
    window.deleteQnA = async () => confirm('Are you sure you want to delete the QnA file?') && (await fetch('/qna/delete', { method: 'DELETE' })).ok && (qnaList.innerHTML = '');
});
