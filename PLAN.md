# خطة التنفيذ: Console Client + Shared Class Library

## 🎯 الهدف

إضافة مشروع Console App يتصل بـ Student API لتجربة الـ API يدوياً باستخدام `HttpClient`، مع إنشاء Class Library مشترك للـ DTOs بين Backend و Console Client.

## 📂 مسار المشروع

`/home/abody81/Downloads/01 StudentAPIServer v7/`

## 🧱 النتيجة المتوقعة

```
StudentApi.sln
├── MyFirstRestAPI_Porject/    ← Backend
│   └── References: StudentApi.Shared
├── StudentApi.Shared/         ← Class Library جديد
│   ├── LoginRequest.cs
│   ├── LoginResponse.cs
│   └── StudentDto.cs
└── StudentApi.Client/         ← Console App جديد
    └── References: StudentApi.Shared
```

## 📝 ملاحظات مهمة

- `StudentDto` لا يحتوي `PasswordHash` — الهدف إخفاء البيانات الحساسة عن API Responses.
- تجاوز SSL في Console Client مخصص للـ Development فقط.
- JWT Token صالح لمدة 30 دقيقة.

## الخطوات

### 1. [x] إنشاء Shared Class Library

في مجلد Solution، نفذ الأوامر التالية:

```bash
dotnet new classlib -n StudentApi.Shared
dotnet sln "StudentApi.sln" add StudentApi.Shared/StudentApi.Shared.csproj
```

**المدخلات:** لا شيء  
**المخرجات:** مشروع `StudentApi.Shared` مضاف للـ Solution.

---

### 2. [x] إنشاء ملفات DTOs في Shared Library

#### `StudentApi.Shared/LoginRequest.cs`

```csharp
namespace StudentApi.Shared;

public record LoginRequest(string Email, string Password);
```

#### `StudentApi.Shared/LoginResponse.cs`

```csharp
namespace StudentApi.Shared;

public record LoginResponse(string Token);
```

#### `StudentApi.Shared/StudentDto.cs`

```csharp
namespace StudentApi.Shared;

public class StudentDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
    public int Grade { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}
```

**المدخلات:** الخطوة 1  
**المخرجات:** 3 ملفات DTOs جاهزة للاستخدام.

---

### 3. [x] حذف النموذج المحلي من Backend

احذف الملف التالي لتجنب التعارض في أسماء الأنواع:

- `/home/abody81/Downloads/01 StudentAPIServer v7/MyFirstRestAPI_Porject/Model/LoginRequest.cs`

**المدخلات:** الخطوة 2  
**المخرجات:** إزالة التعارض بين النماذج المحلية والمشتركة.

---

### 4. [x] إضافة Reference من Backend إلى Shared Library

```bash
dotnet add MyFirstRestAPI_Porject reference StudentApi.Shared
```

**المدخلات:** الخطوة 2  
**المخرجات:** Backend يمكنه استخدام DTOs من Shared Library.

---

### 5. [x] تحديث `AuthController.cs`

**الملف:** `/home/abody81/Downloads/01 StudentAPIServer v7/MyFirstRestAPI_Porject/Controllers/AuthController.cs`

#### التغييرات المطلوبة:

1. استبدال usings:

```csharp
// من:
using Microsoft.AspNetCore.Identity.Data;
using StudentApi.DataSimulation;

// إلى:
using StudentApi.DataSimulation;
using StudentApi.Shared;
```

2. تغيير signature الـ Method:

```csharp
// من:
public IActionResult Loging([FromBody] LoginRequest request)

// إلى:
public IActionResult Login([FromBody] LoginRequest request)
```

3. تغيير الـ return:

```csharp
// من:
return Ok(new
{
    token = new JwtSecurityTokenHandler().WriteToken(token)
});

// إلى:
return Ok(new LoginResponse(new JwtSecurityTokenHandler().WriteToken(token)));
```

**المدخلات:** الخطوة 3 + الخطوة 4  
**المخرجات:** `AuthController` يستخدم Shared DTOs.

---

### 6. [x] تحديث `StudentsController.cs` ليستخدم `StudentDto`

**الملف:** `/home/abody81/Downloads/01 StudentAPIServer v7/MyFirstRestAPI_Porject/Controllers/StudentAPIController.cs`

#### التغييرات المطلوبة:

1. أضف using:

```csharp
using StudentApi.Shared;
```

2. أضف دالة Mapping داخل الـ Controller:

```csharp
private static StudentDto ToDto(Student s) => new()
{
    Id = s.Id,
    Name = s.Name,
    Age = s.Age,
    Grade = s.Grade,
    Email = s.Email,
    Role = s.Role
};
```

3. عدّل الـ return statements:

- **GetAllStudents()**:

```csharp
return Ok(StudentDataSimulation.StudentsList.Select(ToDto).ToList());
```

- **GetPassedStudents()**:

```csharp
return Ok(passedStudents.Select(ToDto).ToList());
```

- **GetStudentById(int id)**:

```csharp
return Ok(ToDto(student));
```

- **AddStudent(Student newStudent)**:

```csharp
return CreatedAtRoute("GetStudentById", new { id = newStudent.Id }, ToDto(newStudent));
```

- **UpdateStudent(int id, Student updatedStudent)**:

```csharp
return Ok(ToDto(student));
```

> ملاحظة: `GetAverageGrade()` لا يحتاج تغيير لأنه يرجع `double`.

**المدخلات:** الخطوة 4  
**المخرجات:** Backend يُرجع `StudentDto` بدلاً من `Student` Entity — لا مزيد من تسرّب `PasswordHash`.

---

### 7. [x] إنشاء Console App

```bash
dotnet new console -n StudentApi.Client
dotnet sln "StudentApi.sln" add StudentApi.Client/StudentApi.Client.csproj
```

**المدخلات:** لا شيء  
**المخرجات:** مشروع Console مضاف للـ Solution.

---

### 8. [x] إضافة References و Package للـ Console App

```bash
dotnet add StudentApi.Client reference StudentApi.Shared
dotnet add StudentApi.Client package System.Net.Http.Json
```

**المدخلات:** الخطوة 2 + الخطوة 7  
**المخرجات:** Console App يمكنه استخدام Shared DTOs و JSON helpers.

---

### 9. [x] كتابة `Program.cs` في Console App

**الملف:** `/home/abody81/Downloads/01 StudentAPIServer v7/StudentApi.Client/Program.cs`

```csharp
using StudentApi.Shared;
using System.Net.Http.Headers;
using System.Net.Http.Json;

Console.WriteLine("=== Student API Console Client ===\n");

// تجاوز SSL certificate في Development فقط
var handler = new HttpClientHandler
{
    ServerCertificateCustomValidationCallback = (_, _, _, _) => true
};

using var client = new HttpClient(handler);
client.BaseAddress = new Uri("https://localhost:7217/");

// 1) Login
Console.Write("Email: ");
var email = Console.ReadLine()!;

Console.Write("Password: ");
var password = Console.ReadLine()!;

var loginResponse = await client.PostAsJsonAsync("api/auth/login", new LoginRequest(email, password));

if (!loginResponse.IsSuccessStatusCode)
{
    Console.WriteLine($"\nLogin failed: {loginResponse.StatusCode}");
    var error = await loginResponse.Content.ReadAsStringAsync();
    Console.WriteLine(error);
    return;
}

var loginData = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
var token = loginData!.Token;

Console.WriteLine($"\nLogin successful.");
Console.WriteLine($"Token: {token[..40]}...\n");

// 2) Call protected endpoint
client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

var studentsResponse = await client.GetAsync("api/Students/All");

if (studentsResponse.IsSuccessStatusCode)
{
    var students = await studentsResponse.Content.ReadFromJsonAsync<List<StudentDto>>();
    Console.WriteLine($"Students count: {students?.Count}\n");

    foreach (var s in students ?? new())
    {
        Console.WriteLine($"- {s.Name} | Age: {s.Age} | Grade: {s.Grade} | Role: {s.Role}");
    }
}
else
{
    Console.WriteLine($"Failed to load students: {studentsResponse.StatusCode}");
}
```

**المدخلات:** الخطوة 8  
**المخرجات:** Console Client جاهز للتشغيل.

---

### 10. [x] بناء Solution للتحقق من الأخطاء

```bash
dotnet build
```

**المدخلات:** جميع الخطوات السابقة  
**المخرجات:** Build ناجح بدون أخطاء.

> **إذا ظهرت أخطاء:**
> - تأكد من حذف `MyFirstRestAPI_Porject/Model/LoginRequest.cs`
> - تأكد من `using StudentApi.Shared;` في `AuthController` و `StudentsController`
> - تأكد من عدم وجود reference قديم لـ `Microsoft.AspNetCore.Identity.Data`

---

### 11. [x] تكوين Multi-Project Startup

#### في Visual Studio:

1. Right-click Solution → Properties
2. Startup Project → Multiple startup projects
3. ضبط:
   - `MyFirstRestAPI_Porject`: **Start**
   - `StudentApi.Client`: **Start** (أو Start without debugging)

#### في VS Code (اختياري):

تعديل `.vscode/launch.json` لإضافة Compound:

```json
{
  "compounds": [
    {
      "name": "API + Client",
      "configurations": ["API", "Client"]
    }
  ]
}
```

**المدخلات:** الخطوة 10  
**المخرجات:** تشغيل Backend و Console معاً بنقرة واحدة.

---

## ✅ خطوات التجربة النهائية

1. شغّل الـ Solution عبر `API + Client` compound.
2. انتظر حتى يعمل Backend على `https://localhost:7217`.
3. في نافذة Console App، أدخل:
   - Email: `ali.ahmed@student.com`
   - Password: `password1`
4. المتوقع:
   - طباعة `Login successful.`
   - طباعة قائمة الطلاب بدون `PasswordHash`.

---

## ⚠️ تحذيرات

1. **تجاوز SSL** في Console Client مخصص للـ Development فقط. لا تتركه في Production.
2. **BaseAddress** يجب أن يطابق port الـ Backend في `launchSettings.json`.
3. **JWT Expiry:** التوكن صالح 30 دقيقة — لو انتهى، أعد تشغيل Console App.
4. **عدم تسرّب PasswordHash:** بعد تنفيذ هذه الخطة، تأكد أن الـ API Response لا يحتوي `passwordHash`.
