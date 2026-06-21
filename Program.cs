using Microsoft.EntityFrameworkCore;
using EquipmentManagement.Data;
using EquipmentManagement.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews(options =>
{
    options.ModelBindingMessageProvider.SetValueMustBeANumberAccessor(_ => "数値を入力してください。");
    options.ModelBindingMessageProvider.SetAttemptedValueIsInvalidAccessor((value, fieldName) => $"{fieldName}の値が不正です。");
    options.ModelBindingMessageProvider.SetMissingBindRequiredValueAccessor(fieldName => $"{fieldName}は必須です。");
    options.ModelBindingMessageProvider.SetMissingKeyOrValueAccessor(() => "入力値が不足しています。");
    options.ModelBindingMessageProvider.SetUnknownValueIsInvalidAccessor(fieldName => $"{fieldName}の値が不正です。");
    options.ModelBindingMessageProvider.SetValueIsInvalidAccessor(value => $"{value}は不正な値です。");
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=equipment.db"));

builder.Services.AddScoped<ItemRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Items/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Items}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();