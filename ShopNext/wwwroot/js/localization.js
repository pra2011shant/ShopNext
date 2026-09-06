/**
 * ShopNext Enterprise Multi-Language & Localization Engine (Point 173)
 * Supports: English, Hindi, Hinglish, Bengali, Marathi, Tamil, Telugu, Gujarati, Kannada, Punjabi
 */

(function () {
    const STORAGE_KEY = 'shopnext_selected_language';
    const COOKIE_NAME = 'ShopNext_Language';

    const LANGUAGES = {
        'en': { name: 'English', native: 'English', flag: '🇬🇧' },
        'hi': { name: 'Hindi', native: 'हिन्दी', flag: '🇮🇳' },
        'hi-Latn': { name: 'Hinglish', native: 'Hinglish', flag: '🇮🇳' },
        'bn': { name: 'Bengali', native: 'বাংলা', flag: '🇮🇳' },
        'mr': { name: 'Marathi', native: 'मराठी', flag: '🇮🇳' },
        'ta': { name: 'Tamil', native: 'தமிழ்', flag: '🇮🇳' },
        'te': { name: 'Telugu', native: 'తెలుగు', flag: '🇮🇳' },
        'gu': { name: 'Gujarati', native: 'ગુજરાતી', flag: '🇮🇳' },
        'kn': { name: 'Kannada', native: 'ಕನ್ನಡ', flag: '🇮🇳' },
        'pa': { name: 'Punjabi', native: 'ਪੰਜਾਬੀ', flag: '🇮🇳' }
    };

    const TRANSLATIONS = {
        'en': {
            'Nav_Home': 'Home',
            'Nav_Marketplace': 'Marketplace',
            'Nav_Shops': 'Explore Shops',
            'Nav_Cart': 'Cart',
            'Nav_Wishlist': 'Wishlist',
            'Nav_MyOrders': 'My Orders',
            'Nav_BecomeSeller': 'Become a Seller',
            'Nav_AdminConsole': 'Admin Console',
            'Nav_SqlHub': 'SQL Hub',
            'Nav_MyProducts': 'My Products',
            'Nav_ShopOrders': 'Shop Orders',
            'Nav_DeliveryDashboard': 'Delivery Dashboard',
            'Nav_MyProfile': 'My Profile',
            'Nav_MyAddresses': 'My Addresses',
            'Nav_Coupons': 'Coupons & Offers',
            'Nav_Wallet': 'Digital Wallet',
            'Nav_Logout': 'Logout Account',
            'Nav_Login': 'Login',
            'Nav_Register': 'Register',
            'Search_Placeholder': 'Search 10,000+ products, brands, or nearby stores...',
            'Hero_Title': 'Next-Generation Hyperlocal Shopping',
            'Hero_Subtitle': 'Get 100% authentic products delivered in minutes from verified neighbourhood merchants.',
            'Btn_AddToCart': 'Add to Cart',
            'Btn_BuyNow': 'Buy Now',
            'Btn_ViewDetails': 'View Details',
            'Btn_Compare': 'Compare',
            'Btn_ApplyCoupon': 'Apply Coupon',
            'Btn_PlaceOrder': 'Place Order',
            'Btn_RetryPayment': 'Retry Payment',
            'Btn_DownloadInvoice': 'Download Tax Invoice',
            'Status_InStock': 'In Stock',
            'Status_OutOfStock': 'Out of Stock',
            'Status_Delivered': 'Delivered',
            'Status_Pending': 'Pending',
            'Status_Processing': 'Processing',
            'Status_Cancelled': 'Cancelled',
            'Status_Returned': 'Returned',
            'Label_TotalAmount': 'Total Amount',
            'Label_Subtotal': 'Subtotal',
            'Label_Discount': 'Discount',
            'Label_DeliveryFee': 'Delivery Fee',
            'Label_TaxGst': 'Estimated GST',
            'Footer_Rights': 'All Rights Reserved',
            'Footer_Privacy': 'Privacy Policy',
            'Footer_Terms': 'Terms of Service',
            'Footer_ReturnPolicy': 'Return & Swap Policy',
            'Footer_Support': '24x7 Customer Grievance Desk',
            'Lang_Select': 'Language'
        },
        'hi': {
            'Nav_Home': 'होम',
            'Nav_Marketplace': 'मार्केटप्लेस',
            'Nav_Shops': 'दुकानें देखें',
            'Nav_Cart': 'कार्ट',
            'Nav_Wishlist': 'विशलिस्ट',
            'Nav_MyOrders': 'मेरे ऑर्डर्स',
            'Nav_BecomeSeller': 'विक्रेता बनें',
            'Nav_AdminConsole': 'एडमिन कंसोल',
            'Nav_SqlHub': 'एसक्यूएल हब',
            'Nav_MyProducts': 'मेरे प्रोडक्ट्स',
            'Nav_ShopOrders': 'दुकान ऑर्डर्स',
            'Nav_DeliveryDashboard': 'डिलीवरी डैशबोर्ड',
            'Nav_MyProfile': 'मेरी प्रोफाइल',
            'Nav_MyAddresses': 'मेरे पते',
            'Nav_Coupons': 'कूपन और ऑफर्स',
            'Nav_Wallet': 'डिजिटल वॉलेट',
            'Nav_Logout': 'लॉगआउट करें',
            'Nav_Login': 'लॉग इन',
            'Nav_Register': 'रजिस्टर करें',
            'Search_Placeholder': '10,000+ प्रोडक्ट्स, ब्रांड्स या नजदीकी दुकानें खोजें...',
            'Hero_Title': 'अगली पीढ़ी का हाइपरलोकल शॉपिंग अनुभव',
            'Hero_Subtitle': 'सत्यापित स्थानीय व्यापारियों से मिनटों में 100% असली उत्पाद प्राप्त करें।',
            'Btn_AddToCart': 'कार्ट में जोड़ें',
            'Btn_BuyNow': 'अभी खरीदें',
            'Btn_ViewDetails': 'विवरण देखें',
            'Btn_Compare': 'तुलना करें',
            'Btn_ApplyCoupon': 'कूपन लागू करें',
            'Btn_PlaceOrder': 'ऑर्डर दें',
            'Btn_RetryPayment': 'भुगतान पुनः प्रयास करें',
            'Btn_DownloadInvoice': 'टैक्स इनवॉइस डाउनलोड करें',
            'Status_InStock': 'स्टॉक में उपलब्ध',
            'Status_OutOfStock': 'स्टॉक समाप्त',
            'Status_Delivered': 'डिलीवर हो गया',
            'Status_Pending': 'लंबित',
            'Status_Processing': 'प्रक्रिया में',
            'Status_Cancelled': 'रद्द किया गया',
            'Status_Returned': 'वापस किया गया',
            'Label_TotalAmount': 'कुल राशि',
            'Label_Subtotal': 'उप-योग',
            'Label_Discount': 'छूट',
            'Label_DeliveryFee': 'डिलीवरी शुल्क',
            'Label_TaxGst': 'अनुमानित जीएसटी',
            'Footer_Rights': 'सर्वाधिकार सुरक्षित',
            'Footer_Privacy': 'गोपनीयता नीति',
            'Footer_Terms': 'सेवा की शर्तें',
            'Footer_ReturnPolicy': 'वापसी और स्वैप नीति',
            'Footer_Support': '24x7 ग्राहक सहायता डेस्क',
            'Lang_Select': 'भाषा चुनें'
        },
        'hi-Latn': {
            'Nav_Home': 'Home',
            'Nav_Marketplace': 'Marketplace',
            'Nav_Shops': 'Dukanein Dekhein',
            'Nav_Cart': 'Cart',
            'Nav_Wishlist': 'Wishlist',
            'Nav_MyOrders': 'Mere Orders',
            'Nav_BecomeSeller': 'Seller Banein',
            'Nav_AdminConsole': 'Admin Console',
            'Nav_SqlHub': 'SQL Hub',
            'Nav_MyProducts': 'Mere Products',
            'Nav_ShopOrders': 'Dukan Orders',
            'Nav_DeliveryDashboard': 'Delivery Dashboard',
            'Nav_MyProfile': 'Meri Profile',
            'Nav_MyAddresses': 'Mere Addresses',
            'Nav_Coupons': 'Coupons & Offers',
            'Nav_Wallet': 'Mera Wallet',
            'Nav_Logout': 'Logout Karein',
            'Nav_Login': 'Login',
            'Nav_Register': 'Register',
            'Search_Placeholder': '10,000+ products, brands ya paas ki dukanein search karein...',
            'Hero_Title': 'Next-Gen Hyperlocal Shopping',
            'Hero_Subtitle': 'Verified local shops se 100% genuine products fast delivery ke saath paayein.',
            'Btn_AddToCart': 'Cart Me Dalein',
            'Btn_BuyNow': 'Abhi Kharidein',
            'Btn_ViewDetails': 'Details Dekhein',
            'Btn_Compare': 'Compare Karein',
            'Btn_ApplyCoupon': 'Coupon Lagayein',
            'Btn_PlaceOrder': 'Order Confirm Karein',
            'Btn_RetryPayment': 'Payment Dobara Karein',
            'Btn_DownloadInvoice': 'Invoice Download Karein',
            'Status_InStock': 'Stock Me Hai',
            'Status_OutOfStock': 'Stock Khatam',
            'Status_Delivered': 'Deliver Ho Gaya',
            'Status_Pending': 'Pending',
            'Status_Processing': 'Processing Me Hai',
            'Status_Cancelled': 'Cancel Hua',
            'Status_Returned': 'Return Hua',
            'Label_TotalAmount': 'Total Amount',
            'Label_Subtotal': 'Subtotal',
            'Label_Discount': 'Discount',
            'Label_DeliveryFee': 'Delivery Charge',
            'Label_TaxGst': 'GST Tax',
            'Footer_Rights': 'Sabhi Adhikar Surakshit',
            'Footer_Privacy': 'Privacy Policy',
            'Footer_Terms': 'Terms of Service',
            'Footer_ReturnPolicy': 'Return & Swap Niti',
            'Footer_Support': '24x7 Customer Helpdesk',
            'Lang_Select': 'Language Chunein'
        },
        'bn': {
            'Nav_Home': 'হোম',
            'Nav_Marketplace': 'মার্কেটপ্লেস',
            'Nav_Shops': 'দোকান খুঁজুন',
            'Nav_Cart': 'কার্ট',
            'Nav_Wishlist': 'উইশলিস্ট',
            'Nav_MyOrders': 'আমার অর্ডার',
            'Nav_BecomeSeller': 'সেলার হন',
            'Nav_AdminConsole': 'অ্যাডমিন কনসোল',
            'Nav_SqlHub': 'এসকিউএল হাব',
            'Nav_MyProducts': 'আমার পণ্য',
            'Nav_ShopOrders': 'দোকানের অর্ডার',
            'Nav_DeliveryDashboard': 'ডেলিভারি ড্যাশবোর্ড',
            'Nav_MyProfile': 'আমার প্রোফাইল',
            'Nav_MyAddresses': 'আমার ঠিকানা',
            'Nav_Coupons': 'কুপন ও অফার',
            'Nav_Wallet': 'ডিজিটাল ওয়ালেট',
            'Nav_Logout': 'লগআউট',
            'Nav_Login': 'লগইন',
            'Nav_Register': 'নিবন্ধন করুন',
            'Search_Placeholder': '১০,০০০+ পণ্য, ব্র্যান্ড বা স্থানীয় দোকান খুঁজুন...',
            'Hero_Title': 'পরবর্তী প্রজন্মের হাইপারলোকাল শপিং',
            'Hero_Subtitle': 'যাচাইকৃত স্থানীয় বিক্রেতাদের কাছ থেকে খাঁটি পণ্য পান দ্রুততম সময়ে।',
            'Btn_AddToCart': 'কার্টে যোগ করুন',
            'Btn_BuyNow': 'এখনই কিনুন',
            'Btn_ViewDetails': 'বিস্তারিত দেখুন',
            'Btn_Compare': 'তুলনা করুন',
            'Btn_ApplyCoupon': 'কুপন প্রয়োগ করুন',
            'Btn_PlaceOrder': 'অর্ডার নিশ্চিত করুন',
            'Btn_RetryPayment': 'পুনরায় পেমেন্ট করুন',
            'Btn_DownloadInvoice': 'চালান ডাউনলোড করুন',
            'Status_InStock': 'স্টকে আছে',
            'Status_OutOfStock': 'স্টক শেষ',
            'Status_Delivered': 'ডেলিভারি সম্পন্ন',
            'Status_Pending': 'মুলতুবি',
            'Status_Processing': 'প্রক্রিয়াকরণ চলছে',
            'Status_Cancelled': 'বাতিল',
            'Status_Returned': 'ফেরত দেওয়া হয়েছে',
            'Label_TotalAmount': 'মোট পরিমাণ',
            'Label_Subtotal': 'সাবটোটাল',
            'Label_Discount': 'ছাড়',
            'Label_DeliveryFee': 'ডেলিভারি চার্জ',
            'Label_TaxGst': 'জিএসটি কর',
            'Footer_Rights': 'সর্বস্বত্ব সংরক্ষিত',
            'Footer_Privacy': 'গোপনীয়তা নীতি',
            'Footer_Terms': 'পরিষেবার শর্তাবলী',
            'Footer_ReturnPolicy': 'রিটার্ন এবং সোয়াপ নীতি',
            'Footer_Support': '২৪x৭ গ্রাহক সহায়তা ডেস্ক',
            'Lang_Select': 'ভাষা নির্বাচন করুন'
        },
        'mr': {
            'Nav_Home': 'मुख्यपृष्ठ',
            'Nav_Marketplace': 'मार्केटप्लेस',
            'Nav_Shops': 'दुकाने पहा',
            'Nav_Cart': 'कार्ट',
            'Nav_Wishlist': 'विशलिस्ट',
            'Nav_MyOrders': 'माझ्या ऑर्डर्स',
            'Nav_BecomeSeller': 'विक्रेता व्हा',
            'Nav_AdminConsole': 'अॅडमिन कन्सोल',
            'Nav_SqlHub': 'एसक्यूएल हब',
            'Nav_MyProducts': 'माझी उत्पादने',
            'Nav_ShopOrders': 'दुकान ऑर्डर्स',
            'Nav_DeliveryDashboard': 'डिलिव्हरी डॅशबोर्ड',
            'Nav_MyProfile': 'माझे प्रोफाइल',
            'Nav_MyAddresses': 'माझे पत्ते',
            'Nav_Coupons': 'कूपन्स आणि ऑफर्स',
            'Nav_Wallet': 'डिजिटल वॉलेट',
            'Nav_Logout': 'लॉगआउट',
            'Nav_Login': 'लॉग इन',
            'Nav_Register': 'नोंदणी करा',
            'Search_Placeholder': '१०,०००+ उत्पादने, ब्रँड्स किंवा दुकाने शोधा...',
            'Hero_Title': 'पुढील पिढीची हायपरलोकल खरेदी',
            'Hero_Subtitle': 'सत्यापित स्थानिक दुकानांमधून अस्सल उत्पादने जलद मिळवा.',
            'Btn_AddToCart': 'कार्टमध्ये टाका',
            'Btn_BuyNow': 'आता खरेदी करा',
            'Btn_ViewDetails': 'तपशील पहा',
            'Btn_Compare': 'तुलना करा',
            'Btn_ApplyCoupon': 'कूपन लागू करा',
            'Btn_PlaceOrder': 'ऑर्डर द्या',
            'Btn_RetryPayment': 'पुन्हा पेमेंट करा',
            'Btn_DownloadInvoice': 'इनव्हॉइस डाउनलोड करा',
            'Status_InStock': 'उपलब्ध आहे',
            'Status_OutOfStock': 'स्टॉक संपला',
            'Status_Delivered': 'डिलिव्हर झाले',
            'Status_Pending': 'प्रलंबित',
            'Status_Processing': 'प्रक्रियेत आहे',
            'Status_Cancelled': 'रद्द केले',
            'Status_Returned': 'परत केले',
            'Label_TotalAmount': 'एकूण रक्कम',
            'Label_Subtotal': 'उप-एकूण',
            'Label_Discount': 'सवलत',
            'Label_DeliveryFee': 'डिलिव्हरी शुल्क',
            'Label_TaxGst': 'जीएसटी कर',
            'Footer_Rights': 'सर्व हक्क राखीव',
            'Footer_Privacy': 'गोपनीयता धोरण',
            'Footer_Terms': 'सेवा अटी',
            'Footer_ReturnPolicy': 'परतावा धोरण',
            'Footer_Support': '२४x७ ग्राहक मदत कक्ष',
            'Lang_Select': 'भाषा निवडा'
        },
        'ta': {
            'Nav_Home': 'முகப்பு',
            'Nav_Marketplace': 'சந்தை',
            'Nav_Shops': 'கடைகளை பார்க்கவும்',
            'Nav_Cart': 'வண்டி',
            'Nav_Wishlist': 'விருப்பப்பட்டியல்',
            'Nav_MyOrders': 'எனது ஆர்டர்கள்',
            'Nav_BecomeSeller': 'விற்பனையாளராகுங்கள்',
            'Nav_AdminConsole': 'நிர்வாகக் கன்சோல்',
            'Nav_SqlHub': 'SQL மையம்',
            'Nav_MyProducts': 'எனது தயாரிப்புகள்',
            'Nav_ShopOrders': 'கடை ஆர்டர்கள்',
            'Nav_DeliveryDashboard': 'டெலிவரி டாஷ்போர்டு',
            'Nav_MyProfile': 'எனது சுயவிவரம்',
            'Nav_MyAddresses': 'எனது முகவரிகள்',
            'Nav_Coupons': 'கூப்பன்கள் & சலுகைகள்',
            'Nav_Wallet': 'டிஜிட்டல் பணப்பை',
            'Nav_Logout': 'வெளியேறு',
            'Nav_Login': 'உள்நுழைக',
            'Nav_Register': 'பதிவு செய்க',
            'Search_Placeholder': '10,000+ தயாரிப்புகள், பிராண்டுகளை தேடுங்கள்...',
            'Hero_Title': 'அடுத்த தலைமுறை ஹைப்பர்லோகல் ஷாப்பிங்',
            'Hero_Subtitle': 'உள்ளூர் வணிகர்களிடமிருந்து 100% உண்மையான தயாரிப்புகளைப் பெறுங்கள்.',
            'Btn_AddToCart': 'கூடையில் சேர்',
            'Btn_BuyNow': 'இப்போது வாங்கு',
            'Btn_ViewDetails': 'விவரங்களைப் பார்க்க',
            'Btn_Compare': 'ஒப்பிடு',
            'Btn_ApplyCoupon': 'கூப்பனைப் பயன்படுத்து',
            'Btn_PlaceOrder': 'ஆர்டர் செய்க',
            'Btn_RetryPayment': 'மீண்டும் பணம் செலுத்துங்கள்',
            'Btn_DownloadInvoice': 'விலைப்பட்டியல் பதிவிறக்கு',
            'Status_InStock': 'கையிருப்பில் உள்ளது',
            'Status_OutOfStock': 'கையிருப்பில் இல்லை',
            'Status_Delivered': 'டெலிவரி செய்யப்பட்டது',
            'Status_Pending': 'நிலுவையில்',
            'Status_Processing': 'செயலாக்கத்தில்',
            'Status_Cancelled': 'ரத்து செய்யப்பட்டது',
            'Status_Returned': 'திரும்பப் பெறப்பட்டது',
            'Label_TotalAmount': 'மொத்தத் தொகை',
            'Label_Subtotal': 'கூட்டுத்தொகை',
            'Label_Discount': 'தள்ளுபடி',
            'Label_DeliveryFee': 'டெலிவரி கட்டணம்',
            'Label_TaxGst': 'ஜிஎஸ்டி வரி',
            'Footer_Rights': 'அனைத்து உரிமைகளும் பாதுகாக்கப்பட்டவை',
            'Footer_Privacy': 'தனியுரிமைக் கொள்கை',
            'Footer_Terms': 'சேவை விதிமுறைகள்',
            'Footer_ReturnPolicy': 'திரும்பப்பெறும் கொள்கை',
            'Footer_Support': '24x7 வாடிக்கையாளர் உதவி',
            'Lang_Select': 'மொழியைத் தேர்ந்தெடுக்கவும்'
        },
        'te': {
            'Nav_Home': 'హోమ్',
            'Nav_Marketplace': 'మార్కెట్ ప్లేస్',
            'Nav_Shops': 'దుకాణాలు చూడండి',
            'Nav_Cart': 'కార్ట్',
            'Nav_Wishlist': 'విష్ లిస్ట్',
            'Nav_MyOrders': 'నా ఆర్డర్లు',
            'Nav_BecomeSeller': 'విక్రేత అవ్వండి',
            'Nav_AdminConsole': 'అడ్మిన్ కన్సోల్',
            'Nav_SqlHub': 'SQL హబ్',
            'Nav_MyProducts': 'నా ఉత్పత్తులు',
            'Nav_ShopOrders': 'దుకాణం ఆర్డర్లు',
            'Nav_DeliveryDashboard': 'డెలివరీ డాష్‌బోర్డ్',
            'Nav_MyProfile': 'నా ప్రొఫైల్',
            'Nav_MyAddresses': 'నా చిరునామాలు',
            'Nav_Coupons': 'కూపన్లు & ఆఫర్లు',
            'Nav_Wallet': 'డిజిటల్ వాలెట్',
            'Nav_Logout': 'లాగౌట్',
            'Nav_Login': 'లాగిన్',
            'Nav_Register': 'రిజిస్టర్',
            'Search_Placeholder': '10,000+ ఉత్పత్తులు, బ్రాండ్‌లు లేదా దుకాణాలను వెతకండి...',
            'Hero_Title': 'తదుపరి తరం హైపర్‌లోకల్ షాపింగ్',
            'Hero_Subtitle': 'ధృవీకరించబడిన స్థానిక విక్రేతల నుండి 100% నిజమైన ఉత్పత్తులను పొందండి.',
            'Btn_AddToCart': 'కార్ట్‌కు జోడించండి',
            'Btn_BuyNow': 'ఇప్పుడే కొనండి',
            'Btn_ViewDetails': 'వివరాలు చూడండి',
            'Btn_Compare': 'పోల్చండి',
            'Btn_ApplyCoupon': 'కూపన్ వర్తింపజేయి',
            'Btn_PlaceOrder': 'ఆర్డర్ చేయండి',
            'Btn_RetryPayment': 'మళ్లీ చెల్లించండి',
            'Btn_DownloadInvoice': 'ఇన్వాయిస్ డౌన్‌లోడ్',
            'Status_InStock': 'స్టాక్ అందుబాటులో ఉంది',
            'Status_OutOfStock': 'స్టాక్ లేదు',
            'Status_Delivered': 'డెలివరీ చేయబడింది',
            'Status_Pending': 'పెండింగ్‌లో ఉంది',
            'Status_Processing': 'ప్రాసెసింగ్‌లో ఉంది',
            'Status_Cancelled': 'రద్దు చేయబడింది',
            'Status_Returned': 'రిటర్న్ చేయబడింది',
            'Label_TotalAmount': 'మొత్తం మొత్తం',
            'Label_Subtotal': 'ఉప మొత్తం',
            'Label_Discount': 'డిస్కౌంట్',
            'Label_DeliveryFee': 'డెలివరీ రుసుము',
            'Label_TaxGst': 'జీఎస్టీ పన్ను',
            'Footer_Rights': 'అన్ని హక్కులు ప్రత్యేకించబడ్డాయి',
            'Footer_Privacy': 'గోప్యతా విధానం',
            'Footer_Terms': 'సేవా నిబంధనలు',
            'Footer_ReturnPolicy': 'రిటర్న్ పాలసీ',
            'Footer_Support': '24x7 కస్టమర్ సపోర్ట్',
            'Lang_Select': 'భాషను ఎంచుకోండి'
        },
        'gu': {
            'Nav_Home': 'હોમ',
            'Nav_Marketplace': 'માર્કેટપ્લેસ',
            'Nav_Shops': 'દુકાનો જુઓ',
            'Nav_Cart': 'કાર્ટ',
            'Nav_Wishlist': 'વિશલિસ્ટ',
            'Nav_MyOrders': 'મારા ઓર્ડર્સ',
            'Nav_BecomeSeller': 'વિક્રેતા બનો',
            'Nav_AdminConsole': 'એડમિન કન્સોલ',
            'Nav_SqlHub': 'SQL હબ',
            'Nav_MyProducts': 'મારા ઉત્પાદનો',
            'Nav_ShopOrders': 'દુકાનના ઓર્ડર',
            'Nav_DeliveryDashboard': 'ડિલિવરી ડેશબોર્ડ',
            'Nav_MyProfile': 'મારી પ્રોફાઇલ',
            'Nav_MyAddresses': 'મારા સરનામાં',
            'Nav_Coupons': 'કૂપન્સ અને ઑફર્સ',
            'Nav_Wallet': 'ડિજિટલ વૉલેટ',
            'Nav_Logout': 'લૉગઆઉટ',
            'Nav_Login': 'લૉગ ઇન',
            'Nav_Register': 'નોંધણી કરો',
            'Search_Placeholder': '10,000+ પ્રોડક્ટ્સ, બ્રાન્ડ્સ અથવા દુકાનો શોધો...',
            'Hero_Title': 'નેક્સ્ટ-જનરેશન હાઇપરલોકલ શોપિંગ',
            'Hero_Subtitle': 'ચકાસાયેલ સ્થાનિક વેપારીઓ પાસેથી મિનિટોમાં 100% અસલી ઉત્પાદનો મેળવો.',
            'Btn_AddToCart': 'કાર્ટમાં ઉમેરો',
            'Btn_BuyNow': 'હમણાં ખરીદો',
            'Btn_ViewDetails': 'વિગતો જુઓ',
            'Btn_Compare': 'સરખામણી કરો',
            'Btn_ApplyCoupon': 'કૂપન લાગુ કરો',
            'Btn_PlaceOrder': 'ઓર્ડર આપો',
            'Btn_RetryPayment': 'ફરીથી ચુકવણી કરો',
            'Btn_DownloadInvoice': 'ઇનવૉઇસ ડાઉનલોડ કરો',
            'Status_InStock': 'સ્ટોકમાં છે',
            'Status_OutOfStock': 'સ્ટોક બહાર',
            'Status_Delivered': 'ડિલિવર થઈ ગયું',
            'Status_Pending': 'બાકી',
            'Status_Processing': 'પ્રક્રિયા હેઠળ',
            'Status_Cancelled': 'રદ થયેલ',
            'Status_Returned': 'પરત કરેલ',
            'Label_TotalAmount': 'કુલ રકમ',
            'Label_Subtotal': 'પેટા-કુલ',
            'Label_Discount': 'ડિસ્કાઉન્ટ',
            'Label_DeliveryFee': 'ડિલિવરી ચાર્જ',
            'Label_TaxGst': 'જીએસટી ટેક્સ',
            'Footer_Rights': 'તમામ હકો અનામત',
            'Footer_Privacy': 'ગોપનીયતા નીતિ',
            'Footer_Terms': 'સેવાની શરતો',
            'Footer_ReturnPolicy': 'રીટર્ન પોલીસી',
            'Footer_Support': '24x7 ગ્રાહક સહાય ડેસ્ક',
            'Lang_Select': 'ભાષા પસંદ કરો'
        },
        'kn': {
            'Nav_Home': 'ಮುಖಪುಟ',
            'Nav_Marketplace': 'ಮಾರುಕಟ್ಟೆ',
            'Nav_Shops': 'ಅಂಗಡಿಗಳನ್ನು ಅನ್ವೇಷಿಸಿ',
            'Nav_Cart': 'ಕಾರ್ಟ್',
            'Nav_Wishlist': 'ಆಶಯಪಟ್ಟಿ',
            'Nav_MyOrders': 'ನನ್ನ ಆದೇಶಗಳು',
            'Nav_BecomeSeller': 'ಮಾರಾಟಗಾರರಾಗಿ',
            'Nav_AdminConsole': 'ಅಡ್ಮಿನ್ ಕನ್ಸೋಲ್',
            'Nav_SqlHub': 'SQL ಹಬ್',
            'Nav_MyProducts': 'ನನ್ನ ಉತ್ಪನ್ನಗಳು',
            'Nav_ShopOrders': 'ಅಂಗಡಿ ಆದೇಶಗಳು',
            'Nav_DeliveryDashboard': 'ಡೆಲಿವರಿ ಡ್ಯಾಶ್‌ಬೋರ್ಡ್',
            'Nav_MyProfile': 'ನನ್ನ ಪ್ರೊಫೈಲ್',
            'Nav_MyAddresses': 'ನನ್ನ ವಿಳಾಸಗಳು',
            'Nav_Coupons': 'ಕೂಪನ್‌ಗಳು ಮತ್ತು ಕೊಡುಗೆಗಳು',
            'Nav_Wallet': 'ಡಿಜಿಟಲ್ ವಾಲೆಟ್',
            'Nav_Logout': 'ಲಾಗ್‌ಔಟ್',
            'Nav_Login': 'ಲಾಗಿನ್',
            'Nav_Register': 'ನೋಂದಾಯಿಸಿ',
            'Search_Placeholder': '10,000+ ಉತ್ಪನ್ನಗಳು, ಬ್ರ್ಯಾಂಡ್‌ಗಳು ಅಥವಾ ಅಂಗಡಿಗಳನ್ನು ಹುಡುಕಿ...',
            'Hero_Title': 'ಮುಂದಿನ ಪೀಳಿಗೆಯ ಹೈಪರ್‌ಲೋಕಲ್ ಶಾಪಿಂಗ್',
            'Hero_Subtitle': 'ಸ್ಥಳೀಯ ವ್ಯಾಪಾರಿಗಳಿಂದ 100% ಅಧಿಕೃತ ಉತ್ಪನ್ನಗಳನ್ನು ತ್ವರಿತವಾಗಿ ಪಡೆಯಿರಿ.',
            'Btn_AddToCart': 'ಕಾರ್ಟ್‌ಗೆ ಸೇರಿಸಿ',
            'Btn_BuyNow': 'ಈಗ ಖರೀದಿಸಿ',
            'Btn_ViewDetails': 'ವಿವರಗಳನ್ನು ವೀಕ್ಷಿಸಿ',
            'Btn_Compare': 'ಹೋಲಿಕೆ ಮಾಡಿ',
            'Btn_ApplyCoupon': 'ಕೂಪನ್ ಅನ್ವಯಿಸಿ',
            'Btn_PlaceOrder': 'ಆದೇಶ ನೀಡಿ',
            'Btn_RetryPayment': 'ಮತ್ತೆ ಪಾವತಿಸಿ',
            'Btn_DownloadInvoice': 'ಇನ್‌ವಾಯ್ಸ್ ಡೌನ್‌ಲೋಡ್',
            'Status_InStock': 'ದಾಸ್ತಾನು ಇದೆ',
            'Status_OutOfStock': 'ದಾಸ್ತಾನು ಮುಗಿದಿದೆ',
            'Status_Delivered': 'ತಲುಪಿಸಲಾಗಿದೆ',
            'Status_Pending': 'ಬಾಕಿ ಇದೆ',
            'Status_Processing': 'ಪ್ರಕ್ರಿಯೆಯಲ್ಲಿದೆ',
            'Status_Cancelled': 'ರದ್ದುಗೊಳಿಸಲಾಗಿದೆ',
            'Status_Returned': 'ಹಿಂತಿರುಗಿಸಲಾಗಿದೆ',
            'Label_TotalAmount': 'ಒಟ್ಟು ಮೊತ್ತ',
            'Label_Subtotal': 'ಉಪ-ಒಟ್ಟು',
            'Label_Discount': 'ರಿಯಾಯಿತಿ',
            'Label_DeliveryFee': 'ಡೆಲಿವರಿ ಶುಲ್ಕ',
            'Label_TaxGst': 'ಜಿಎಸ್‌ಟಿ ತೆರಿಗೆ',
            'Footer_Rights': 'ಎಲ್ಲಾ ಹಕ್ಕುಗಳನ್ನು ಕಾಯ್ದಿರಿಸಲಾಗಿದೆ',
            'Footer_Privacy': 'ಗೌಪ್ಯತಾ ನೀತಿ',
            'Footer_Terms': 'ಸೇವಾ ನಿಯಮಗಳು',
            'Footer_ReturnPolicy': 'ಹಿಂತಿರುಗಿಸುವ ನೀತಿ',
            'Footer_Support': '24x7 ಗ್ರಾಹಕ ಬೆಂಬಲ',
            'Lang_Select': 'ಭಾಷೆಯನ್ನು ಆಯ್ಕೆಮಾಡಿ'
        },
        'pa': {
            'Nav_Home': 'ਮੁੱਖ ਪੰਨਾ',
            'Nav_Marketplace': 'ਮਾਰਕੀਟਪਲੇਸ',
            'Nav_Shops': 'ਦੁਕਾਨਾਂ ਦੇਖੋ',
            'Nav_Cart': 'ਕਾਰਟ',
            'Nav_Wishlist': 'ਵਿਸ਼ਲਿਸਟ',
            'Nav_MyOrders': 'ਮੇਰੇ ਆਰਡਰ',
            'Nav_BecomeSeller': 'ਵਿਕਰੇਤਾ ਬਣੋ',
            'Nav_AdminConsole': 'ਐਡਮਿਨ ਕੰਸੋਲ',
            'Nav_SqlHub': 'SQL ਹੱਬ',
            'Nav_MyProducts': 'ਮੇਰੇ ਉਤਪਾਦ',
            'Nav_ShopOrders': 'ਦੁਕਾਨ ਦੇ ਆਰਡਰ',
            'Nav_DeliveryDashboard': 'ਡਿਲੀਵਰੀ ਡੈਸ਼ਬੋਰਡ',
            'Nav_MyProfile': 'ਮੇਰੀ ਪ੍ਰੋਫਾਈਲ',
            'Nav_MyAddresses': 'ਮੇਰੇ ਪਤੇ',
            'Nav_Coupons': 'ਕੂਪਨ ਅਤੇ ਪੇਸ਼ਕਸ਼ਾਂ',
            'Nav_Wallet': 'ਡਿਜੀਟਲ ਵਾਲਿਟ',
            'Nav_Logout': 'ਲਾਗਆਉਟ',
            'Nav_Login': 'ਲਾਗਇਨ',
            'Nav_Register': 'ਰਜਿਸਟਰ',
            'Search_Placeholder': '10,000+ ਉਤਪਾਦ, ਬ੍ਰਾਂਡ ਜਾਂ ਦੁਕਾਨਾਂ ਖੋਜੋ...',
            'Hero_Title': 'ਅਗਲੀ ਪੀੜ੍ਹੀ ਦੀ ਹਾਈਪਰਲੋਕਲ ਖਰੀਦਦਾਰੀ',
            'Hero_Subtitle': 'ਪ੍ਰਮਾਣਿਤ ਸਥਾਨਕ ਵਪਾਰੀਆਂ ਤੋਂ ਮਿੰਟਾਂ ਵਿੱਚ 100% ਅਸਲੀ ਉਤਪਾਦ ਪ੍ਰਾਪਤ ਕਰੋ।',
            'Btn_AddToCart': 'ਕਾਰਟ ਵਿੱਚ ਸ਼ਾਮਲ ਕਰੋ',
            'Btn_BuyNow': 'ਹੁਣੇ ਖਰੀਦੋ',
            'Btn_ViewDetails': 'ਵੇਰਵੇ ਦੇਖੋ',
            'Btn_Compare': 'ਤੁਲਨਾ ਕਰੋ',
            'Btn_ApplyCoupon': 'ਕੂਪਨ ਲਾਗੂ ਕਰੋ',
            'Btn_PlaceOrder': 'ਆਰਡਰ ਕਰੋ',
            'Btn_RetryPayment': 'ਦੁਬਾਰਾ ਭੁਗਤਾਨ ਕਰੋ',
            'Btn_DownloadInvoice': 'ਇਨਵੌਇਸ ਡਾਊਨਲੋਡ ਕਰੋ',
            'Status_InStock': 'ਸਟਾਕ ਵਿੱਚ ਹੈ',
            'Status_OutOfStock': 'ਸਟਾਕ ਖਤਮ',
            'Status_Delivered': 'ਡਿਲੀਵਰ ਹੋ ਗਿਆ',
            'Status_Pending': 'ਬਕਾਇਆ',
            'Status_Processing': 'ਕਾਰਵਾਈ ਅਧੀਨ',
            'Status_Cancelled': 'ਰੱਦ ਕੀਤਾ ਗਿਆ',
            'Status_Returned': 'ਵਾਪਸ ਕੀਤਾ ਗਿਆ',
            'Label_TotalAmount': 'ਕੁੱਲ ਰਕਮ',
            'Label_Subtotal': 'ਉਪ-ਕੁੱਲ',
            'Label_Discount': 'ਛੋਟ',
            'Label_DeliveryFee': 'ਡਿਲੀਵਰੀ ਚਾਰਜ',
            'Label_TaxGst': 'ਜੀਐਸਟੀ ਟੈਕਸ',
            'Footer_Rights': 'ਸਾਰੇ ਹੱਕ ਰਾਖਵੇਂ ਹਨ',
            'Footer_Privacy': 'ਗੋਪਨੀਯਤਾ ਨੀਤੀ',
            'Footer_Terms': 'ਸੇਵਾ ਦੀਆਂ ਸ਼ਰਤਾਂ',
            'Footer_ReturnPolicy': 'ਵਾਪਸੀ ਨੀਤੀ',
            'Footer_Support': '24x7 ਗਾਹਕ ਸਹਾਇਤਾ ਡੈਸਕ',
            'Lang_Select': 'ਭਾਸ਼ਾ ਚੁਣੋ'
        }
    };

    function getCookie(name) {
        const match = document.cookie.match(new RegExp('(^| )' + name + '=([^;]+)'));
        return match ? decodeURIComponent(match[2]) : null;
    }

    function setCookie(name, value, days = 365) {
        const d = new Date();
        d.setTime(d.getTime() + (days * 24 * 60 * 60 * 1000));
        document.cookie = `${name}=${encodeURIComponent(value)};expires=${d.toUTCString()};path=/;SameSite=Lax`;
    }

    function getCurrentLanguage() {
        const cookieLang = getCookie(COOKIE_NAME);
        if (cookieLang && TRANSLATIONS[cookieLang]) return cookieLang;
        const storedLang = localStorage.getItem(STORAGE_KEY);
        if (storedLang && TRANSLATIONS[storedLang]) return storedLang;
        return 'en';
    }

    function translateText(key, lang = null) {
        const current = lang || getCurrentLanguage();
        const dict = TRANSLATIONS[current] || TRANSLATIONS['en'];
        return dict[key] || TRANSLATIONS['en'][key] || key;
    }

    let isTranslating = false;

    function applyTranslations(lang) {
        if (isTranslating) return;
        isTranslating = true;

        try {
            const targetLang = lang || getCurrentLanguage();
            const dict = TRANSLATIONS[targetLang] || TRANSLATIONS['en'];

            // 1. Elements with data-i18n
            document.querySelectorAll('[data-i18n]').forEach(el => {
                const key = el.getAttribute('data-i18n');
                if (!key || !dict[key]) return;
                const newText = dict[key];

                if (el.tagName === 'INPUT' || el.tagName === 'TEXTAREA') {
                    if (el.getAttribute('placeholder') !== newText) {
                        el.setAttribute('placeholder', newText);
                    }
                } else {
                    // Update only text child or text content without destroying icons
                    const icon = el.querySelector('i');
                    if (icon) {
                        // Find text node after icon
                        let textNode = null;
                        for (let i = 0; i < el.childNodes.length; i++) {
                            const node = el.childNodes[i];
                            if (node.nodeType === Node.TEXT_NODE && node.nodeValue.trim().length > 0) {
                                textNode = node;
                                break;
                            }
                        }
                        if (textNode) {
                            if (textNode.nodeValue.trim() !== newText) {
                                textNode.nodeValue = ' ' + newText;
                            }
                        } else {
                            const span = el.querySelector('.i18n-text');
                            if (span) {
                                if (span.innerText !== newText) span.innerText = newText;
                            } else {
                                const newSpan = document.createElement('span');
                                newSpan.className = 'i18n-text ms-1';
                                newSpan.innerText = newText;
                                el.appendChild(newSpan);
                            }
                        }
                    } else {
                        if (el.innerText.trim() !== newText) {
                            el.innerText = newText;
                        }
                    }
                }
            });

            // 2. Search inputs with data-i18n-placeholder
            document.querySelectorAll('[data-i18n-placeholder]').forEach(el => {
                const key = el.getAttribute('data-i18n-placeholder');
                if (key && dict[key] && el.getAttribute('placeholder') !== dict[key]) {
                    el.setAttribute('placeholder', dict[key]);
                }
            });

            // 3. Update active language badge / text in navbar dropdown
            const activeFlag = document.getElementById('activeLangFlag');
            const activeName = document.getElementById('activeLangName');
            const langInfo = LANGUAGES[targetLang] || LANGUAGES['en'];

            if (activeFlag && activeFlag.innerText !== langInfo.flag) activeFlag.innerText = langInfo.flag;
            if (activeName && activeName.innerText !== langInfo.native) activeName.innerText = langInfo.native;

            // Highlight selected dropdown item
            document.querySelectorAll('.lang-dropdown-item').forEach(item => {
                const itemLang = item.getAttribute('data-lang');
                if (itemLang === targetLang) {
                    item.classList.add('active', 'bg-primary', 'bg-opacity-25', 'fw-bold');
                } else {
                    item.classList.remove('active', 'bg-primary', 'bg-opacity-25', 'fw-bold');
                }
            });

            document.documentElement.setAttribute('lang', targetLang);
        } catch (e) {
            console.error('Localization error:', e);
        } finally {
            isTranslating = false;
        }
    }

    window.switchLanguage = function (langCode, notify = true) {
        if (!LANGUAGES[langCode]) langCode = 'en';

        localStorage.setItem(STORAGE_KEY, langCode);
        setCookie(COOKIE_NAME, langCode, 365);

        applyTranslations(langCode);

        // Notify server asynchronously so server-side rendered views get the new culture cookie
        try {
            fetch(`/Home/SetLanguageJson?culture=${encodeURIComponent(langCode)}`, { method: 'POST' });
        } catch (e) { }

        if (notify) {
            const langName = LANGUAGES[langCode].native;
            if (typeof showToast === 'function') {
                showToast(`🌐 Language changed to ${langName}`);
            }
        }
    };

    // Initialize on DOM ready
    document.addEventListener('DOMContentLoaded', function () {
        const lang = getCurrentLanguage();
        applyTranslations(lang);
    });

    // Expose ShopNext i18n API to window
    window.ShopNextI18n = {
        translate: translateText,
        getCurrentLanguage: getCurrentLanguage,
        switchLanguage: window.switchLanguage,
        applyTranslations: applyTranslations,
        languages: LANGUAGES
    };
})();
