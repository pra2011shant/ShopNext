// ShopNext - Core JavaScript Suite, UI Helpers, & Recently Viewed Manager

// ==========================================================================
// 1. GLOBAL TOAST NOTIFICATION SYSTEM
// ==========================================================================
function showToast(message, type = 'info', duration = 3200) {
    let container = document.getElementById("globalToastContainer");
    if (!container) {
        container = document.createElement("div");
        container.id = "globalToastContainer";
        document.body.appendChild(container);
    }

    const toast = document.createElement("div");
    toast.className = `shopnext-toast ${type}`;

    let icon = "fa-circle-info text-primary";
    if (type === "success") icon = "fa-circle-check text-success";
    if (type === "error" || type === "danger") icon = "fa-circle-xmark text-danger";
    if (type === "warning") icon = "fa-triangle-exclamation text-warning";

    toast.innerHTML = `
        <i class="fa-solid ${icon} fs-5"></i>
        <div class="flex-grow-1">${message}</div>
        <button type="button" class="btn-close btn-close-sm" style="font-size: 0.75rem;" aria-label="Close"></button>
    `;

    const closeBtn = toast.querySelector(".btn-close");
    closeBtn.addEventListener("click", () => {
        toast.classList.remove("show");
        setTimeout(() => toast.remove(), 300);
    });

    container.appendChild(toast);

    // Trigger animation
    requestAnimationFrame(() => {
        toast.classList.add("show");
    });

    // Auto dismiss
    setTimeout(() => {
        if (toast.parentElement) {
            toast.classList.remove("show");
            setTimeout(() => toast.remove(), 300);
        }
    }, duration);
}
window.showToast = showToast;

// ==========================================================================
// 2. GLOBAL LOADING SPINNER
// ==========================================================================
function showLoadingSpinner(message = "Loading, please wait...") {
    let overlay = document.getElementById("globalSpinnerOverlay");
    if (!overlay) {
        overlay = document.createElement("div");
        overlay.id = "globalSpinnerOverlay";
        overlay.innerHTML = `
            <div class="spinner-pulse-ring mb-3"></div>
            <div id="globalSpinnerText" class="text-white fw-bold fs-6">Loading, please wait...</div>
        `;
        document.body.appendChild(overlay);
    }
    const txt = document.getElementById("globalSpinnerText");
    if (txt) txt.innerText = message;
    overlay.classList.add("active");
}

function hideLoadingSpinner() {
    const overlay = document.getElementById("globalSpinnerOverlay");
    if (overlay) {
        overlay.classList.remove("active");
    }
}
window.showLoadingSpinner = showLoadingSpinner;
window.hideLoadingSpinner = hideLoadingSpinner;

// ==========================================================================
// 3. GLOBAL CONFIRMATION MODAL
// ==========================================================================
let globalConfirmCallback = null;

function confirmAction(options) {
    const modalEl = document.getElementById("globalConfirmationModal");
    if (!modalEl) {
        if (window.confirm(options.message || "Are you sure?")) {
            if (options.onConfirm) options.onConfirm();
        }
        return;
    }

    const titleEl = document.getElementById("globalConfirmTitle");
    const msgEl = document.getElementById("globalConfirmMessage");
    const iconEl = document.getElementById("globalConfirmIcon");
    const submitBtn = document.getElementById("globalConfirmSubmitBtn");

    if (titleEl) titleEl.innerText = options.title || "Confirmation Required";
    if (msgEl) msgEl.innerHTML = options.message || "Are you sure you want to proceed with this action?";
    
    if (iconEl) {
        iconEl.className = options.icon || "fa-solid fa-triangle-exclamation text-warning fa-2x mb-3";
    }

    if (submitBtn) {
        submitBtn.innerText = options.confirmText || "Confirm";
        submitBtn.className = `btn ${options.confirmClass || 'btn-danger'}`;
    }

    globalConfirmCallback = options.onConfirm || null;

    const bsModal = bootstrap.Modal.getOrCreateInstance(modalEl);
    bsModal.show();
}
window.confirmAction = confirmAction;

document.addEventListener("DOMContentLoaded", function () {
    const confirmBtn = document.getElementById("globalConfirmSubmitBtn");
    if (confirmBtn) {
        confirmBtn.addEventListener("click", function () {
            const modalEl = document.getElementById("globalConfirmationModal");
            if (modalEl) {
                const bsModal = bootstrap.Modal.getInstance(modalEl);
                if (bsModal) bsModal.hide();
            }
            if (typeof globalConfirmCallback === "function") {
                const cb = globalConfirmCallback;
                globalConfirmCallback = null;
                cb();
            }
        });
    }
});

// ==========================================================================
// 4. RECENTLY VIEWED PRODUCTS MANAGER
// ==========================================================================
const ShopNextRecentlyViewed = {
    STORAGE_KEY: "shop_recently_viewed",

    DEFAULT_ITEMS: [
        {
            id: 14,
            name: "Samsung Mobile Galaxy M34 5G",
            category: "Electronics",
            price: 16999.00,
            imageUrl: "https://images.unsplash.com/photo-1610945265064-0e34e5519bbf?w=500&auto=format&fit=crop&q=60",
            shopName: "ElectroHub Digital Store",
            shopId: 3,
            stockStatus: "InStock",
            rating: 4.8
        },
        {
            id: 15,
            name: "HP Slim Core i5 Laptop 16GB RAM",
            category: "Electronics",
            price: 44990.00,
            imageUrl: "https://images.unsplash.com/photo-1496181133206-80ce9b88a853?w=500&auto=format&fit=crop&q=60",
            shopName: "ElectroHub Digital Store",
            shopId: 3,
            stockStatus: "InStock",
            rating: 4.9
        },
        {
            id: 16,
            name: "Sony Over-Ear Wireless Headphone ANC",
            category: "Electronics",
            price: 4999.00,
            imageUrl: "https://images.unsplash.com/photo-1505740420928-5e560c06d30e?w=500&auto=format&fit=crop&q=60",
            shopName: "ElectroHub Digital Store",
            shopId: 3,
            stockStatus: "InStock",
            rating: 4.7
        },
        {
            id: 17,
            name: "Nike Air Cushion Running Shoes",
            category: "Fashion",
            price: 2499.00,
            imageUrl: "https://images.unsplash.com/photo-1542291026-7eec264c27ff?w=500&auto=format&fit=crop&q=60",
            shopName: "Green Mart Superstore",
            shopId: 1,
            stockStatus: "InStock",
            rating: 4.8
        }
    ],

    getItems: function () {
        try {
            const raw = localStorage.getItem(this.STORAGE_KEY);
            if (!raw) {
                localStorage.setItem(this.STORAGE_KEY, JSON.stringify(this.DEFAULT_ITEMS));
                return this.DEFAULT_ITEMS;
            }
            const parsed = JSON.parse(raw);
            return Array.isArray(parsed) ? parsed : this.DEFAULT_ITEMS;
        } catch (e) {
            console.error("Error reading recently viewed items", e);
            return this.DEFAULT_ITEMS;
        }
    },

    recordProduct: function (product) {
        if (!product || !product.id) return;
        let items = this.getItems();
        items = items.filter(i => i.id !== product.id);

        items.unshift({
            id: product.id,
            name: product.name,
            category: product.category || "General",
            price: Number(product.price) || 0,
            imageUrl: product.imageUrl || "",
            shopName: product.shopName || "Shop",
            shopId: product.shopId || 1,
            stockStatus: product.stockStatus || "InStock",
            rating: product.rating || 4.8,
            viewedAt: new Date().toISOString()
        });

        if (items.length > 10) {
            items = items.slice(0, 10);
        }

        try {
            localStorage.setItem(this.STORAGE_KEY, JSON.stringify(items));
        } catch (e) {
            console.error("Error saving recently viewed item", e);
        }
    },

    clear: function (containerId = null, sectionId = null) {
        try {
            localStorage.setItem(this.STORAGE_KEY, JSON.stringify([]));
            if (containerId) {
                const container = document.getElementById(containerId);
                if (container) container.innerHTML = '';
            }
            if (sectionId) {
                const section = document.getElementById(sectionId);
                if (section) section.style.display = 'none';
            }
            showToast("Recently viewed history cleared.", "info");
        } catch (e) {
            console.error("Error clearing recently viewed items", e);
        }
    },

    render: function (containerId, sectionId = null, excludeId = null) {
        const container = document.getElementById(containerId);
        if (!container) return;

        let items = this.getItems();
        if (excludeId) {
            items = items.filter(i => i.id !== Number(excludeId));
        }

        const section = sectionId ? document.getElementById(sectionId) : null;

        if (!items || items.length === 0) {
            if (section) section.style.display = 'none';
            container.innerHTML = '';
            return;
        }

        if (section) section.style.display = 'block';

        let html = '';
        items.forEach(p => {
            const inStock = p.stockStatus === 'InStock';
            const escapedName = (p.name || '').replace(/'/g, "\\'");
            const escapedShop = (p.shopName || 'Shop').replace(/'/g, "\\'");

            html += `
                <div class="col-6 col-md-4 col-lg-3">
                    <div class="recently-viewed-card h-100 d-flex flex-column justify-content-between">
                        <div>
                            <div class="position-relative overflow-hidden rounded-3 mb-2" style="height: 140px; background: #0f172a; border: 1px solid rgba(255, 255, 255, 0.08);">
                                ${p.imageUrl ?
                                    `<img src="${p.imageUrl}" alt="${p.name}" class="w-100 h-100" style="object-fit: cover; transition: transform 0.3s ease;" onerror="this.src='https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=500&auto=format&fit=crop&q=60';" />`
                                    : `<div class="w-100 h-100 d-flex align-items-center justify-content-center text-muted"><i class="fa-solid fa-box-open fa-2x opacity-50"></i></div>`
                                }
                                <span class="badge ${inStock ? 'bg-success bg-opacity-25 text-success border border-success border-opacity-30' : 'bg-danger bg-opacity-25 text-danger border border-danger border-opacity-30'} position-absolute top-0 start-0 m-2 px-2 py-1 rounded-pill" style="font-size: 0.7rem;">
                                    ${inStock ? 'In Stock' : 'Out of Stock'}
                                </span>
                                <span class="badge bg-dark bg-opacity-80 text-light position-absolute top-0 end-0 m-2 px-2 py-1 rounded-pill border border-secondary border-opacity-40" style="font-size: 0.7rem;">
                                    ${p.category || 'General'}
                                </span>
                            </div>
                            <a href="/Home/ProductDetails/${p.id}" class="text-decoration-none">
                                <h6 class="fw-bold mb-1 text-truncate text-white" title="${p.name}">${p.name}</h6>
                            </a>
                            <div class="d-flex justify-content-between align-items-center mb-1">
                                <span class="text-secondary small text-truncate" style="max-width: 140px;">
                                    <i class="fa-solid fa-store text-warning me-1"></i>${p.shopName || 'Shop'}
                                </span>
                                <span class="text-warning small fw-semibold">
                                    <i class="fa-solid fa-star"></i> ${p.rating ? Number(p.rating).toFixed(1) : '4.8'}
                                </span>
                            </div>
                            <div class="fs-5 fw-bold text-success mb-2 font-monospace">
                                &#8377;${Number(p.price).toFixed(2)}
                            </div>
                        </div>
                        <div class="d-flex gap-2 pt-2 border-top border-secondary border-opacity-25">
                            <a href="/Home/ProductDetails/${p.id}" class="btn btn-outline-info btn-sm flex-grow-1 rounded-pill">
                                <i class="fa-solid fa-eye me-1"></i> View
                            </a>
                            <button class="btn btn-primary btn-sm flex-grow-1 rounded-pill" onclick="ShopNextRecentlyViewed.addToCart(${p.id}, '${escapedName}', ${p.price}, ${p.shopId}, '${escapedShop}')">
                                <i class="fa-solid fa-cart-plus"></i> Add
                            </button>
                        </div>
                    </div>
                </div>
            `;
        });

        container.innerHTML = html;
    },

    addToCart: function (productId, productName, price, shopId, shopName) {
        let cart = JSON.parse(localStorage.getItem("shop_cart")) || { shopId: null, shopName: null, items: [] };

        if (cart.shopId && cart.shopId !== shopId && cart.items.length > 0) {
            confirmAction({
                title: "Clear Current Cart?",
                message: `Your cart currently contains items from <b>"${cart.shopName}"</b>. Would you like to clear it and add products from <b>"${shopName}"</b> instead?`,
                icon: "fa-solid fa-cart-shopping text-warning fa-2x mb-3",
                confirmText: "Clear & Add New",
                confirmClass: "btn-primary",
                onConfirm: () => {
                    this._proceedAddToCart(productId, productName, price, shopId, shopName, true);
                }
            });
            return;
        }

        this._proceedAddToCart(productId, productName, price, shopId, shopName, false);
    },

    _proceedAddToCart: function (productId, productName, price, shopId, shopName, resetCart) {
        let cart = resetCart ? { shopId: shopId, shopName: shopName, items: [] } : (JSON.parse(localStorage.getItem("shop_cart")) || { shopId: shopId, shopName: shopName, items: [] });
        cart.shopId = shopId;
        cart.shopName = shopName;

        const existing = cart.items.find(i => i.productId === productId);
        if (existing) {
            existing.quantity += 1;
        } else {
            cart.items.push({
                productId: productId,
                productName: productName,
                price: price,
                quantity: 1
            });
        }

        localStorage.setItem("shop_cart", JSON.stringify(cart));

        let totalQty = 0;
        cart.items.forEach(i => totalQty += i.quantity);
        if (window.updateLayoutCartBadge) {
            window.updateLayoutCartBadge(totalQty);
        }

        showToast(`1x "<b>${productName}</b>" added to your cart!`, "success");
    }
};
