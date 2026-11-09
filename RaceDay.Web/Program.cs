using RaceDay.Web.Services;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

//the MVC app remembers who is logged in and which role they have using session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{//start of session options
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    //its own cookie name, because browsers share cookies between ports on localhost and the API also uses a session cookie
    options.Cookie.Name = ".RaceDay.Web.Session";
});//end of session options

builder.Services.AddHttpContextAccessor();

//one shared client for talking to the API; ApiClient handles the login cookie itself, so the built-in cookie handling is switched off
builder.Services.AddHttpClient<ApiClient>()
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { UseCookies = false });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
