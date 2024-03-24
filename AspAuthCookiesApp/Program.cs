using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;


var builder = WebApplication.CreateBuilder(args);

List<User> users = new()
{
    new(){ Login = "bobby", Password = "qwerty" },
    new(){ Login = "tommy", Password = "12345" },
};

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options => options.LoginPath = "/login");
builder.Services.AddAuthorization();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/login", async (HttpContext context) =>
{
    context.Response.ContentType = "text/html; charset=utf-8";
    string loginForm = @"
    <form method='post'>
        <p>
            <label>login</label><br />
            <input type='text' id='login' name='login' />
        </p>
        <p>
            <label>password</label><br />
            <input type='password' id='password' name='password'/>
        </p>
        <input type='submit' id='btn_submit' value='Log In' />
    </form>";
    await context.Response.WriteAsync(loginForm);
});

app.MapPost("/login", async (string? url, HttpContext context) =>
{
    var form = context.Request.Form;

    if (!form.ContainsKey("login") || !form.ContainsKey("password"))
        return Results.BadRequest("Login or password not found");

    var login = form["login"];
    var password = form["password"];

    User? user = users.FirstOrDefault(u => u.Login == login && u.Password == password);
    if (user is null)
        return Results.Unauthorized();

    List<Claim> claims = new()
    {
        new Claim(ClaimTypes.Name, user.Login)
    };

    ClaimsIdentity identity = new(claims, "Coockie");

    await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                               new ClaimsPrincipal(identity));

    return Results.Redirect(url ?? "/");

});

app.MapGet("/logout", async (HttpContext context) =>
{
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Redirect("/login");
});

app.MapGet("/", [Authorize] () => "Hello world");

app.Run();


class User
{
    public string Login { get; set; } = "";
    public string Password { get; set; } = "";
}
