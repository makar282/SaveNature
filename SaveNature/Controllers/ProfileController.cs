//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Mvc;
//using SaveNature.Models;
//using System.Threading.Tasks;

//public class ProfileController : Controller
//{
//    private readonly UserManager<IdentityUser> _userManager;
//    private readonly SignInManager<IdentityUser> _signInManager;

//    public ProfileController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
//    {
//        _userManager = userManager;
//        _signInManager = signInManager;
//    }

//    public IActionResult Index()
//    {
//        // Логика для отображения текущей страницы профиля
//        return View(new ProfileViewModel()); // Замените на вашу текущую логику
//    }

//    public async Task<IActionResult> EditProfile()
//    {
//        var user = await _userManager.GetUserAsync(User);
//        if (user == null)
//        {
//            return RedirectToAction("Login", "Account");
//        }

//        var model = new EditProfileViewModel
//        {
//            UserName = user.UserName,
//            Password = user.PasswordHash,
//            AboutMe = user.UserName // Здесь можно загрузить текущее описание, если оно хранится
//        };

//        return View(model);
//    }

//    [HttpPost]
//    [ValidateAntiForgeryToken]
//    public async Task<IActionResult> EditProfile(EditProfileViewModel model)
//    {
//        if (!ModelState.IsValid)
//        {
//            return View(model);
//        }

//        var user = await _userManager.GetUserAsync(User);
//        if (user == null)
//        {
//            return RedirectToAction("Login", "Account");
//        }

//        // Обновление имени пользователя
//        if (model.UserName != user.UserName)
//        {
//            var setUserNameResult = await _userManager.SetUserNameAsync(user, model.UserName);
//            if (!setUserNameResult.Succeeded)
//            {
//                foreach (var error in setUserNameResult.Errors)
//                {
//                    ModelState.AddModelError(string.Empty, error.Description);
//                }
//                return View(model);
//            }
//        }

//        // Обновление пароля
//        if (!string.IsNullOrEmpty(model.Password))
//        {
//            var removePasswordResult = await _userManager.RemovePasswordAsync(user);
//            if (!removePasswordResult.Succeeded)
//            {
//                foreach (var error in removePasswordResult.Errors)
//                {
//                    ModelState.AddModelError(string.Empty, error.Description);
//                }
//                return View(model);
//            }

//            var addPasswordResult = await _userManager.AddPasswordAsync(user, model.Password);
//            if (!addPasswordResult.Succeeded)
//            {
//                foreach (var error in addPasswordResult.Errors)
//                {
//                    ModelState.AddModelError(string.Empty, error.Description);
//                }
//                return View(model);
//            }
//        }

//        // Обновление описания "Обо мне" (если оно хранится в базе, например, в кастомной таблице)
//        // Здесь нужно добавить логику для сохранения model.AboutMe в базу данных

//        // Обновляем сессию, чтобы отобразить новое имя пользователя
//        await _signInManager.RefreshSignInAsync(user);

//        return RedirectToAction("Index");
//    }
//}