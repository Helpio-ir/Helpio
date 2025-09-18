# Ticket Details Page - JavaScript & CSS Organization

## 📁 File Structure

```
src/Presentation/Helpio.Dashboard/wwwroot/
├── js/tickets/
│   └── ticket-details.js     # JavaScript functionality for ticket details page
├── css/tickets/
│   └── ticket-details.css    # CSS styles for ticket details page
└── Views/Tickets/
    └── Details.cshtml         # Main view file (cleaner, less inline code)
```

## 🎯 Purpose

این refactoring برای بهبود سازماندهی کد و آسان‌تر کردن debug انجام شده است.

## ✨ Features

### JavaScript Functions (ticket-details.js)

- **loadCannedResponses()** - نمایش modal پاسخ‌های آماده
- **selectCannedResponse()** - انتخاب و اعمال پاسخ آماده
- **searchCannedResponses()** - جستجو در پاسخ‌های آماده
- **insertVariable()** - وارد کردن متغیرهای پویا
- **updateCannedResponseUsage()** - به‌روزرسانی تعداد استفاده
- **showToast()** - نمایش پیام‌های نوتیفیکیشن
- **escapeHtml()** - جلوگیری از XSS attacks

### CSS Styles (ticket-details.css)

- **Conversation Thread** - استایل‌های گفت‌وگو و پاسخ‌ها
- **Timeline Styles** - زمان‌بندی تیکت
- **Modal Styles** - پاسخ‌های آماده و متغیرها
- **Responsive Design** - پشتیبانی از موبایل
- **Accessibility** - پشتیبانی از high contrast و reduced motion

## 🔧 Usage

### در فایل Details.cshtml:

```html
@section Styles {
    <link rel="stylesheet" href="~/css/tickets/ticket-details.css" />
}

@section Scripts {
    <script>
        // Set API URLs for JavaScript
        window.cannedResponsesApiUrl = '@Url.Action("GetCannedResponses", "Knowledge")';
        window.incrementUsageApiUrl = '@Url.Action("IncrementCannedResponseUsage", "Knowledge")';
    </script>
    <script src="~/js/tickets/ticket-details.js"></script>
}
```

## 🚀 Benefits

1. **Better Organization** - کد جاوااسکریپت و CSS از HTML جدا شده
2. **Easier Debugging** - خطاها در فایل‌های جداگانه قابل ردیابی
3. **Code Reusability** - امکان استفاده مجدد در صفحات مشابه
4. **Better Performance** - امکان caching فایل‌های static
5. **Maintainability** - تعمیر و نگهداری آسان‌تر

## 🛠️ Debugging

### Browser DevTools:
- JavaScript errors حالا در `ticket-details.js` نمایش داده می‌شوند
- CSS issues در `ticket-details.css` قابل ردیابی است
- Network tab برای بررسی API calls

### Console Logs:
```javascript
console.log('Ticket Details page initialized');
console.log('Calling canned responses API:', url);
console.log('Received data:', data);
```

## 📱 Responsive Features

- Mobile-friendly design
- Touch-friendly buttons
- Optimized modal sizes for small screens
- Accessible navigation

## ♿ Accessibility

- High contrast mode support
- Reduced motion preferences
- Screen reader friendly
- Keyboard navigation support

## 🔐 Security

- HTML escaping for user content
- XSS prevention
- Safe API calls with proper headers

## 📋 API Integration

فایل JavaScript با این API endpoints ارتباط برقرار می‌کند:

- `GET /Knowledge/GetCannedResponses` - دریافت پاسخ‌های آماده
- `POST /Knowledge/IncrementCannedResponseUsage` - افزایش شمارنده استفاده

## 🧪 Testing

برای تست کردن:

1. صفحه Details تیکت را باز کنید
2. دکمه "پاسخ‌های آماده" را کلیک کنید
3. Console log ها را در DevTools مشاهده کنید
4. عملکرد responsive را بررسی کنید

## 📝 Notes

- فایل‌های CSS و JS در build process include می‌شوند
- API URLs به صورت پویا از server تنظیم می‌شوند
- Fallback data برای زمان خطا در API در نظر گرفته شده