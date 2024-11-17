namespace Zilean.Shared.Features.Torznab.Categories;

public static class TorznabCategoryTypes
{
    public static TorznabCategory Console => new(1000, "Console")
    {
        SubCategories =
        {
            ConsoleNDS,
            ConsolePSP,
            ConsoleWii,
            ConsoleXBox,
            ConsoleXBox360,
            ConsoleWiiware,
            ConsoleXBox360Dlc,
            ConsolePs3,
            ConsoleOther,
            Console3DS,
            ConsolePSVita,
            ConsoleWiiU,
            ConsoleXBoxOne,
            ConsolePS4
        }
    };

    public static TorznabCategory Movies => new(2000, "Movies")
    {
        SubCategories =
        {
            MoviesForeign,
            MoviesOther,
            MoviesSD,
            MoviesHD,
            MoviesUHD,
            MoviesBluRay,
            Movies3D,
            MoviesDVD,
            MoviesWEBDL
        }
    };

    public static TorznabCategory Audio => new(3000, "Audio")
    {
        SubCategories =
        {
            AudioMp3,
            AudioVideo,
            AudioAudiobook,
            AudioLossless,
            AudioOther,
            AudioForeign
        }
    };

    public static TorznabCategory PC => new(4000, "PC")
    {
        SubCategories =
        {
            PC0day,
            PCISO,
            PCMac,
            PCMobileOther,
            PCGames,
            PCMobileiOS,
            PCMobileAndroid
        }
    };

    public static TorznabCategory TV => new(5000, "TV")
    {
        SubCategories =
        {
            TVWEBDL,
            TVForeign,
            TVSD,
            TVHD,
            TVUHD,
            TVOther,
            TVSport,
            TVAnime,
            TVDocumentary
        }
    };

    public static TorznabCategory XXX => new(6000, "XXX")
    {
        SubCategories =
        {
            XXXDVD,
            XXX,
            XXXXviD,
            XXXx264,
            XXXUHD,
            XXXPack,
            XXXImageSet,
            XXXOther,
            XXXSD,
            XXXWEBDL
        }
    };

        public static TorznabCategory Books => new(7000, "Books")
    {
        SubCategories =
        {
            BooksMags,
            BooksEBook,
            BooksComics,
            BooksTechnical,
            BooksOther,
            BooksForeign
        }
    };

    public static TorznabCategory Other => new(8000, "Other")
    {
        SubCategories =
        {
            OtherMisc,
            OtherHashed
        }
    };

    public static TorznabCategory[] ParentCats =>
    [
        Console,
        Movies,
        Audio,
        PC,
        TV,
        XXX,
        Books,
        Other
    ];

    public static TorznabCategory[] AllCats =>
    [
        Console,
        ConsoleNDS,
        ConsolePSP,
        ConsoleWii,
        ConsoleXBox,
        ConsoleXBox360,
        ConsoleWiiware,
        ConsoleXBox360Dlc,
        ConsolePs3,
        ConsoleOther,
        Console3DS,
        ConsolePSVita,
        ConsoleWiiU,
        ConsoleXBoxOne,
        ConsolePS4,
        Movies,
        MoviesForeign,
        MoviesOther,
        MoviesSD,
        MoviesHD,
        MoviesUHD,
        MoviesBluRay,
        Movies3D,
        MoviesDVD,
        MoviesWEBDL,
        Audio,
        AudioMp3,
        AudioVideo,
        AudioAudiobook,
        AudioLossless,
        AudioOther,
        AudioForeign,
        PC,
        PC0day,
        PCISO,
        PCMac,
        PCMobileOther,
        PCGames,
        PCMobileiOS,
        PCMobileAndroid,
        TV,
        TVWEBDL,
        TVForeign,
        TVSD,
        TVHD,
        TVUHD,
        TVOther,
        TVSport,
        TVAnime,
        TVDocumentary,
        XXX,
        XXXDVD,
        XXXWMV,
        XXXXviD,
        XXXx264,
        XXXUHD,
        XXXPack,
        XXXImageSet,
        XXXOther,
        XXXSD,
        XXXWEBDL,
        Books,
        BooksMags,
        BooksEBook,
        BooksComics,
        BooksTechnical,
        BooksOther,
        BooksForeign,
        Other,
        OtherMisc,
        OtherHashed
    ];

    public static TorznabCategory ConsoleNDS => new(1010, "Console/NDS");
    public static TorznabCategory ConsolePSP => new(1020, "Console/PSP");
    public static TorznabCategory ConsoleWii => new(1030, "Console/Wii");
    public static TorznabCategory ConsoleXBox => new(1040, "Console/XBox");
    public static TorznabCategory ConsoleXBox360 => new(1050, "Console/XBox 360");
    public static TorznabCategory ConsoleWiiware => new(1060, "Console/Wiiware");
    public static TorznabCategory ConsoleXBox360Dlc => new(1070, "Console/XBox 360 DLC");
    public static TorznabCategory ConsolePs3 => new(1080, "Console/PS3");
    public static TorznabCategory ConsoleOther => new(1090, "Console/Other");
    public static TorznabCategory Console3DS => new(1110, "Console/3DS");
    public static TorznabCategory ConsolePSVita => new(1120, "Console/PS Vita");
    public static TorznabCategory ConsoleWiiU => new(1130, "Console/WiiU");
    public static TorznabCategory ConsoleXBoxOne => new(1140, "Console/XBox One");
    public static TorznabCategory ConsolePS4 => new(1180, "Console/PS4");
    public static TorznabCategory MoviesForeign => new(2010, "Movies/Foreign");
    public static TorznabCategory MoviesOther => new(2020, "Movies/Other");
    public static TorznabCategory MoviesSD => new(2030, "Movies/SD");
    public static TorznabCategory MoviesHD => new(2040, "Movies/HD");
    public static TorznabCategory MoviesUHD => new(2045, "Movies/UHD");
    public static TorznabCategory MoviesBluRay => new(2050, "Movies/BluRay");
    public static TorznabCategory Movies3D => new(2060, "Movies/3D");
    public static TorznabCategory MoviesDVD => new(2070, "Movies/DVD");
    public static TorznabCategory MoviesWEBDL => new(2080, "Movies/WEB-DL");
    public static TorznabCategory AudioMp3 => new(3010, "Audio/MP3");
    public static TorznabCategory AudioVideo => new(3020, "Audio/Video");
    public static TorznabCategory AudioAudiobook => new(3030, "Audio/Audiobook");
    public static TorznabCategory AudioLossless => new(3040, "Audio/Lossless");
    public static TorznabCategory AudioOther => new(3050, "Audio/Other");
    public static TorznabCategory AudioForeign => new(3060, "Audio/Foreign");
    public static TorznabCategory PC0day => new(4010, "PC/0day");
    public static TorznabCategory PCISO => new(4020, "PC/ISO");
    public static TorznabCategory PCMac => new(4030, "PC/Mac");
    public static TorznabCategory PCMobileOther => new(4040, "PC/Mobile-Other");
    public static TorznabCategory PCGames => new(4050, "PC/Games");
    public static TorznabCategory PCMobileiOS => new(4060, "PC/Mobile-iOS");
    public static TorznabCategory PCMobileAndroid => new(4070, "PC/Mobile-Android");
    public static TorznabCategory TVWEBDL => new(5010, "TV/WEB-DL");
    public static TorznabCategory TVForeign => new(5020, "TV/Foreign");
    public static TorznabCategory TVSD => new(5030, "TV/SD");
    public static TorznabCategory TVHD => new(5040, "TV/HD");
    public static TorznabCategory TVUHD => new(5045, "TV/UHD");
    public static TorznabCategory TVOther => new(5050, "TV/Other");
    public static TorznabCategory TVSport => new(5060, "TV/Sport");
    public static TorznabCategory TVAnime => new(5070, "TV/Anime");
    public static TorznabCategory TVDocumentary => new(5080, "TV/Documentary");
    public static TorznabCategory XXXDVD => new(6010, "XXX/DVD");
    public static TorznabCategory XXXWMV => new(6020, "XXX/WMV");
    public static TorznabCategory XXXXviD => new(6030, "XXX/XviD");
    public static TorznabCategory XXXx264 => new(6040, "XXX/x264");
    public static TorznabCategory XXXUHD => new(6045, "XXX/UHD");
    public static TorznabCategory XXXPack => new(6050, "XXX/Pack");
    public static TorznabCategory XXXImageSet => new(6060, "XXX/ImageSet");
    public static TorznabCategory XXXOther => new(6070, "XXX/Other");
    public static TorznabCategory XXXSD => new(6080, "XXX/SD");
    public static TorznabCategory XXXWEBDL => new(6090, "XXX/WEB-DL");
    public static TorznabCategory BooksMags => new(7010, "Books/Mags");
    public static TorznabCategory BooksEBook => new(7020, "Books/EBook");
    public static TorznabCategory BooksComics => new(7030, "Books/Comics");
    public static TorznabCategory BooksTechnical => new(7040, "Books/Technical");
    public static TorznabCategory BooksOther => new(7050, "Books/Other");
    public static TorznabCategory BooksForeign => new(7060, "Books/Foreign");
    public static TorznabCategory OtherMisc => new(8010, "Other/Misc");
    public static TorznabCategory OtherHashed => new(8020, "Other/Hashed");

    public static string GetCatDesc(int torznabCatId) =>
        AllCats.FirstOrDefault(c => c.Id == torznabCatId)?.Name ?? string.Empty;

    public static TorznabCategory GetCatByName(string name) => AllCats.FirstOrDefault(c => c.Name == name);
}
