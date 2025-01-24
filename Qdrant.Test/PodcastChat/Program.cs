using PodcastChat.Components;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

/***
 * TODO:
 * 1. - multiple back and forth with the user - make it more nice - components, scrollable, etc.
 * 2. - in response put a link or media player to play the fragment
 * https://demos.themeselection.com/sneat-bootstrap-html-admin-template/documentation/extended-ui-media-player.html
 * 3. use actual search and actual timestamps
 * 4. process CRC podcast (or Doug podcast)
 * 5. add semantic kernel to the search
 */
