const Default = {
    maxFileSize: Infinity,
    defaultFile: null,
    height: null,
    disabled: false,
    acceptedExtensions: [],
    multiple: false,
    defaultMsg: 'Drag and drop a file here or click',
    mainError: 'Ooops, something wrong happended.',
    maxSizeError: 'Your file is too big (Max size ~~~)',
    formatError: 'Your file has incorrect file format (correct format(s) ~~~)',
    quantityError: 'Too many files (allowed quantity of files ~~~)',
    previewMsg: 'Drag and drop or click to replace',
    removeBtn: 'Remove',
    disabledRemoveBtn: false,
    maxFileQuantity: Infinity
};

const UnitTypes = {
    G: 1000000000,
    M: 1000000,
    K: 1000,
    B: 1,
};

const NAME = 'fileUpload';
const DATA_KEY = `datakey.${NAME}`;
const EVENT_KEY = `.${DATA_KEY}`;
const MAX_UID = 1000000;

const EVENT_ERROR = `fileError${EVENT_KEY}`;
const EVENT_FILE_REMOVE = `fileRemove${EVENT_KEY}`;
const EVENT_FILE_ADD = `fileAdd${EVENT_KEY}`;

const getUID = (prefix) => {
    do {
        prefix += Math.floor(Math.random() * MAX_UID);
    } while (document.getElementById(prefix));

    return prefix;
};
class FileUpload {
    constructor(element, options = {}) {
        this._element = element;
        this.options = this._getConfig(options)
        this._fileUploadWrapper = this._element.parentNode;
        this._files = [];
        this._errors = [];

        if (this._element) {
            this._element.dataset[DATA_KEY] = this;
        }

        this.init();
    }
    static get NAME() {
        return NAME;
    }

    init() {
        this._createNativeAttr();
        this._createDropZone();
        this._element.addEventListener('change', (e) => {
            const isMultiple = this.options.multiple;
            if (isMultiple) {
                this._createMultipleList(e);
            } else {
                this._files = Array.from(e.target.files);
            }
            this._onChangeEvent();
        });
        if (this.options.isUpdate && this._files !== []) {
            this._element.dispatchEvent('change');
        }
    }

    dispose() {
        this._element.removeEventListener('change');
        this._element.removeEventListener('click');
        const fileUploadPreviews = document.querySelectorAll('.file-upload-previews', this._fileUploadWrapper);
        fileUploadPreviews.forEach(el => {
            el.removeEventListener('drop');
            el.removeEventListener('dragover');
        });
        delete this._element.dataset[DATA_KEY];
        this._element = null;
    }

    reset() {
        this._files = [];
        this._errors = [];
        this._createDropZone();
    }

    update(newOptions = {}) {
        this.options = this._getConfig(newOptions);
        this._createNativeAttr();
        this._createDropZone();
    }

    _getConfig(options) {
        const config = { ...Default, ...options };
        if (config.maxFileSize !== Infinity) {
            config.maxFileSize = convertFileSizeToBytes(config.maxFileSize);
        }
        if (typeof config.acceptedExtensions === 'string') {
            config.acceptedExtensions = config.acceptedExtensions.split(',');
        }
        return config;
    }

    _createNativeAttr() {
        const { disabled, acceptedExtensions, multiple } = this.options;

        if (disabled) {
            this._element.setAttribute('disabled', disabled);
        }
        if (acceptedExtensions) {
            this._element.setAttribute('accept', acceptedExtensions);
        }
        if (multiple) {
            this._element.setAttribute('multiple', multiple);
        }
    }

    _createDropZone() {
        this._createBasicContainer();
        if (this.options.defaultFile) {
            this._createDefaultFilePreview();
        }
    }

    _createBasicContainer() {

        const existingFileUpload = document.querySelector('.file-upload', this._fileUploadWrapper);
        if (existingFileUpload) {
            this._fileUploadWrapper.removeChild(existingFileUpload);
        }

        const fileUpload = document.createElement('div');
        fileUpload.className = 'file-upload';

        if (this.options.height) {
            fileUpload.style.height = `${this.options.height}px`;
        }
        if (this.options.disabled) {
            fileUpload.classList.add('disabled');
        }

        const msgContainer = this._createFileUploadMsg();
        const fileUploadErrors = this._createFileUploadErrors();
        const fileUploadMask = this._createFileUploadMask();
        const fileUploadPreviews = this._createPreviews();

        if (this.options.multiple) {
            fileUpload.classList.add('has-multiple');
        }

        fileUpload.appendChild(msgContainer);
        fileUpload.appendChild(fileUploadMask);
        fileUpload.appendChild(fileUploadErrors);
        fileUpload.appendChild(this._element);
        fileUpload.appendChild(fileUploadPreviews);

        this._fileUploadWrapper.appendChild(fileUpload);
    }

    _createFileUploadMsg() {
        const msgContainer = document.createElement('div');
        msgContainer.className = 'file-upload-message';

        const cloudIco = document.createElement('i');
        cloudIco.className = 'fas fa-cloud-upload-alt file-upload-cloud-icon';
        const defaultMsg = document.createElement('p');
        defaultMsg.className = 'file-upload-default-message';
        defaultMsg.textContent = this.options.defaultMsg;

        const mainError = document.createElement('p');
        mainError.className = 'file-upload-main-error';

        msgContainer.appendChild(cloudIco);
        msgContainer.appendChild(defaultMsg);
        msgContainer.appendChild(mainError);

        return msgContainer;
    }

    _createFileUploadErrors() {
        const errorsContainer = document.createElement('ul');
        errorsContainer.className = 'file-upload-errors';

        return errorsContainer;
    }

    _createFileUploadMask() {
        const fileUploadMask = document.createElement('div');
        fileUploadMask.className = 'file-upload-mask';

        return fileUploadMask;
    }

    _createPreviews() {
        const fileUploadPreviews = document.createElement('div');
        fileUploadPreviews.className = 'file-upload-previews';

        if (this.options.multiple) {
            fileUploadPreviews.addEventListener('drop', (e) => {
                e.preventDefault();
                if (this.options.maxFileQuantity > this._files.length) {
                    const files = e.dataTransfer ? Array.from(e.dataTransfer.files) : [];
                    files.forEach((file) => {
                        file.id = getUID('file-');
                    });
                    this._files = [...this._files, ...files];
                    this._onChangeEvent();
                }
            });

            fileUploadPreviews.addEventListener('dragover', (e) => {
                e.preventDefault();
            });
        }

        return fileUploadPreviews;
    }

    _createDefaultFilePreview() {
        const fileUploadPreviews = document.querySelector('.file-upload-previews', this._fileUploadWrapper);
        const fileUpload = document.querySelector('.file-upload', this._fileUploadWrapper);
        fileUpload.classList.add('has-preview');

        const preview = document.createElement('div');
        preview.className = 'file-upload-preview';

        const renderContainer = document.createElement('span');
        renderContainer.className = 'file-upload-render';

        const fileUploadPreviewDetails = document.createElement('div');
        fileUploadPreviewDetails.className = 'file-upload-preview-details';

        let removeBtn;
        if (!this.options.disabledRemoveBtn) {
            removeBtn = this._createClearButton(preview);
        }

        const detailsInner = document.createElement('div');
        detailsInner.className = 'file-uplod-preview-details-inner';

        const fileNameContainer = document.createElement('p');
        fileNameContainer.className = 'file-upload-file-name';

        const fileInfo = this.options.defaultFile.split('/');
        const fileName = fileInfo[fileInfo.length - 1];
        const fileType = fileName.split('.')[fileName.split('.').length - 1];

        if (
            fileType === 'jpg' ||
            fileType === 'jpeg' ||
            fileType === 'png' ||
            fileType === 'svg' ||
            fileType === 'webp' ||
            fileType === 'bmp' ||
            fileType === 'gif'
        ) {
            const fileUploadPreviewImg = document.createElement('img');
            fileUploadPreviewImg.className = 'file-upload-preview-img';
            fileUploadPreviewImg.src = this.options.defaultFile;
            renderContainer.appendChild(fileUploadPreviewImg);
        } else {
            const fileIco = document.createElement('i');
            fileIco.className = 'fas fa-file file-upload-file-icon';
            const extension = document.createElement('span');
            extension.className = 'file-upload-extension';
            renderContainer.appendChild(fileIco);
            renderContainer.appendChild(extension);
        }

        fileNameContainer.textContent = fileName;

        const previewMsg = document.createElement('p');
        previewMsg.className = 'file-upload-preview-message';
        previewMsg.textContent = this.options.previewMsg;

        detailsInner.appendChild(fileNameContainer);
        detailsInner.appendChild(previewMsg);
        if (removeBtn) {
            fileUploadPreviewDetails.appendChild(removeBtn);
        }
        fileUploadPreviewDetails.appendChild(detailsInner);

        preview.appendChild(renderContainer);
        preview.appendChild(fileUploadPreviewDetails);

        fileUploadPreviews.appendChild(preview);
    }

    _createMultipleList(event) {
        const canUploadMoreFiles = this.options.maxFileQuantity >= event.target.files.length;
        if (canUploadMoreFiles) {
            this._files = [...this._files, ...Array.from(event.target.files)];
        }
    }

    _onChangeEvent() {
        this._validateParameters();
        if (this._errors.length) {
            this._files = [];
            this._element.value = '';

            // Assuming you have an event object defined elsewhere
            const event = new CustomEvent(EVENT_ERROR, {
                detail: { errors: this._errors },
                bubbles: true, // Set bubbles to true for event propagation
            });
            this._element.dispatchEvent(event);
        } else {
            this._createFilesId();

            // Assuming you have an event object defined elsewhere
            const event = new CustomEvent(EVENT_FILE_ADD, {
                detail: { files: this._files },
                bubbles: true, // Set bubbles to true for event propagation
            });

            this._element?.dispatchEvent(event);
        }
        this._createPreview();
        this._errors = [];
    }

    _validateParameters() {
        this._files.forEach(file => {
            this._checkFileSize(file);
            this._checkAcceptedExtensions(file);
        });
    }

    _checkFileSize(file) {
        const { maxFileSize, maxSizeError } = this.options;
        const BYTES_IN_MEGABYTE = UnitTypes.M; // Assuming UnitTypes.M is defined elsewhere

        if (maxFileSize < file.size) {
            this._errors.push(maxSizeError.replace('~~~', `${maxFileSize / BYTES_IN_MEGABYTE}M`));
            if (this.options.multiple) {
                this._files = this._files.filter(currentFile => currentFile.id !== file.id);
            }
        }
    }

    _checkAcceptedExtensions(file) {
        const { acceptedExtensions, invalidExtensionError } = this.options;
        const allowedExtensions = acceptedExtensions.map(ext => ext.trim().toLowerCase()); // Normalize extensions
        const fileExtension = file.name.split('.').pop().toLowerCase(); // Extract and lowercase file extension

        if (!allowedExtensions.includes(fileExtension)) {
            this._errors.push(invalidExtensionError.replace('~~~', fileExtension));
            if (this.options.multiple) {
                this._files = this._files.filter(currentFile => currentFile.id !== file.id);
            }
        }
    }

    _createFilesId() {
        this._files.forEach(file => {
            file.id = getUID('file-'); // Assuming getUID function exists
        });
    }

    _createPreview() {
        const fileUpload = document.querySelector('.file-upload', this._fileUploadWrapper);
        const fileUploadPreviews = document.querySelector('.file-upload-previews', this._fileUploadWrapper);

        // Handle single file case
        if (!this.options.isMultiple) {
            const fileUploadPreviewElements = document.querySelectorAll('.file-upload-preview', fileUploadPreviews);
            for (let element of fileUploadPreviewElements) {
                fileUploadPreviews.removeChild(element);
            }
        }

        // Handle errors
        if (this._errors.length) {
            fileUpload.classList.add('has-error');
            const errorsContainer = fileUpload.querySelector('ul.file-upload-errors');
            errorsContainer.innerHTML = '';
            document.querySelector('.file-upload-main-error', fileUpload).textContent = this.options.mainError;

            for (const error of this._errors) {
                const errorElement = document.createElement('li');
                errorElement.className = 'file-upload-error';
                errorElement.textContent = error;
                errorsContainer.appendChild(errorElement);
            }
            return;
        }

        // Clear previous previews and errors
        fileUpload.classList.remove('has-error');
        const errorsContainer = fileUpload.querySelector('ul.file-upload-errors');
        errorsContainer.innerHTML = '';

        // Handle file previews
        for (const file of this._files) {
            fileUpload.classList.add('has-preview');

            const fileUploadPreview = document.createElement('div');
            fileUploadPreview.className = 'file-upload-preview';

            const fileUploadRender = this._createFileUploadRender(file);
            const fileUploadPreviewDetails = this._createFileUploadPreviewDetails(file, fileUploadPreview);

            fileUploadPreview.appendChild(fileUploadRender);
            fileUploadPreview.appendChild(fileUploadPreviewDetails);
            fileUploadPreviews.appendChild(fileUploadPreview);
        }
    }

    _createFileUploadRender(file) {
        const types = file.type.split('/');
        const fileUploadRender = document.createElement('div');
        fileUploadRender.className = 'file-upload-render';

        if (types[0] === 'image') {
            const reader = new FileReader();
            reader.onloadend = function () {
                const imgEl = document.createElement('img');
                imgEl.src = this.result;
                imgEl.className = 'file-upload-preview-img';
                fileUploadRender.appendChild(imgEl);
            };
            if (file) {
                reader.readAsDataURL(file);
            }
        } else {
            const fileIco = document.createElement('i');
            fileIco.className = 'fas fa-file file-upload-file-icon';
            const fileUploadExtension = document.createElement('span');
            fileUploadExtension.className = 'file-upload-extension';
            fileUploadExtension.textContent = types[1];

            fileUploadRender.appendChild(fileIco);
            fileUploadRender.appendChild(fileUploadExtension);
        }

        return fileUploadRender;
    }

    _createFileUploadPreviewDetails(file, fileUploadPreview) {
        const previewDetails = document.createElement('div');
        previewDetails.className = 'file-upload-preview-details';

        const detailsContainer = document.createElement('div');
        detailsContainer.className = 'file-upload-details-container';

        if (this.options.multiple) {
            detailsContainer.addEventListener('click', () => {
                if (this.options.maxFileQuantity > this._files.length) {
                    this._element?.click()
                }
            });
        }

        const detailsInner = document.createElement('div');
        detailsInner.className = 'file-uplod-preview-details-inner';

        const fileName = document.createElement('p');
        fileName.className = 'file-upload-file-name';
        fileName.textContent = file.name;

        const previewMsg = document.createElement('p');
        previewMsg.className = 'file-upload-preview-message';
        previewMsg.textContent = this.options.previewMsg;

        detailsInner.appendChild(fileName);
        detailsInner.appendChild(previewMsg);
        detailsContainer.appendChild(detailsInner);

        if (!this.options.disabledRemoveBtn) {
            previewDetails.appendChild(this._createClearButton(fileUploadPreview, file));
        }

        previewDetails.appendChild(detailsContainer);

        return previewDetails;
    }

    _createClearButton(currentPreviewEl, file) {
        const { removeBtn } = this.options;
        const clearButton = document.createElement('button');
        clearButton.className = 'btn btn-danger file-upload-remove-file-btn';
        clearButton.textContent = removeBtn;
        clearButton.dataset.rippleInit = ''; // Set data attribute directly

        const trashIco = document.createElement('i');
        trashIco.className = 'far fa-trash-alt ms-1';
        clearButton.appendChild(trashIco);

        clearButton.addEventListener('click', () => {
            const parent = currentPreviewEl.parentNode;
            this._removeFileAndPreview(currentPreviewEl, parent, file);
        });

        return clearButton;
    }

    _removeFileAndPreview(currentPreviewEl, parent, file) {
        const fileUpload = document.querySelector('.file-upload', this._fileUploadWrapper);
        parent.removeChild(currentPreviewEl);

        if (this.options.multiple) {
            this._files = this._files.filter((el) => el.id !== file.id);
        } else {
            this._files = [];
        }
        this._element.value = '';
        this._errors = [];
        fileUpload.classList.remove('has-error');

        // Assuming you have an event object defined elsewhere
        const event = new CustomEvent(EVENT_FILE_REMOVE, {
            detail: {
                files: this._files,
                removedFile: file,
            },
            // bubbles: true, // Set bubbles to true for event propagation
        });
        this._element.dispatchEvent(event);
    }

    static getInstance(element) {
        return element.dataset[DATA_KEY]; // Access data attribute directly
    }

}

