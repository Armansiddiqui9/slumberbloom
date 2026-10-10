using Slumberbloom.Contracts;
using Slumberbloom.UI;

/// <summary>
/// The main loop: onboarding → bedtime → dew → garden/census.
/// Mirrors the web app's App.tsx render + tab navigation (nav buttons at bottom).
/// </summary>
public sealed class App
{
    private readonly AppModel _model;
    private ScreenId _screen = ScreenId.Onboarding;

    public App(IGameApi api) => _model = new AppModel { Api = api };

    public async Task RunAsync()
    {
        // Onboarding (first run)
        if (_screen == ScreenId.Onboarding)
        {
            var seed = await OnboardingScreen.RunAsync(_model.Api);
            _model.SelectedSeedId = seed;
            _screen = ScreenId.Bedtime;
        }

        // Main loop
        while (true)
        {
            await _model.RefreshAsync();

            switch (_screen)
            {
                case ScreenId.Bedtime:
                    BedtimeScreen.Render(_model);
                    _screen = ScreenId.MorningDew; // natural next step after tuck-in
                    break;
                case ScreenId.MorningDew:
                    MorningDewScreen.Render(_model);
                    // After claim/dawn, go to garden for the daily ritual
                    if (_model.Blocks.Any(b => b.Stage == BlockStage.Frost || b.Stage == BlockStage.Gathered))
                        _screen = ScreenId.Garden;
                    else if (_model.Blocks.Any(b => b.Stage == BlockStage.Dew))
                        _screen = ScreenId.MorningDew;
                    else
                        _screen = ScreenId.Bedtime;
                    break;
                case ScreenId.Garden:
                    GardenScreen.Render(_model);
                    _screen = ScreenId.Census;
                    break;
                case ScreenId.Census:
                    CensusScreen.Render(_model);
                    Console.WriteLine();
                    Console.WriteLine("  [Q] Quit  [B] Bedtime  [D] Dew  [G] Garden  [C] Census");
                    var k = Console.ReadKey(true).KeyChar;
                    if (k == 'q' || k == 'Q') return;
                    else if (k == 'b' || k == 'B') _screen = ScreenId.Bedtime;
                    else if (k == 'd' || k == 'D') _screen = ScreenId.MorningDew;
                    else if (k == 'g' || k == 'G') _screen = ScreenId.Garden;
                    else if (k == 'c' || k == 'C') _screen = ScreenId.Census;
                    break;
            }

            await Task.Delay(50); // yield
        }
    }
}
