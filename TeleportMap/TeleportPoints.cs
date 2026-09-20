using FFXIVClientStructs.FFXIV.Component.GUI;
using System.Collections.Generic;
using System.Numerics;

namespace TeleportMap;

public record TeleportPoint(uint AetheryteId, Vector2 Position, uint IconId = TeleportPoints.DefaultIcon, int? UldPart = null, byte SubIndex = 0);
public static class TeleportPoints
{
    public const uint DefaultIcon = 60453;

    public static readonly List<TeleportPoint> All =
    [
                // ===================== The Source (Hauptkarte) =====================
 
        // --- La Noscea ---
        new(8, new(1140, 1288), UldPart: 2),          // Limsa Lominsa Lower Decks [Stadt]
        new(52, new(1180, 1288)),                     // Summerford Farms
        new(10, new(1220, 1310)),                     // Moraby Drydocks
        new(11, new(1270, 1260)),                     // Costa del Sol
        new(12, new(1242, 1230)),                     // Wineport
        new(13, new(1180, 1240)),                     // Swiftperch
        new(14, new(1150, 1230)),                     // Aleport
        new(15, new(1210, 1196)),                     // Camp Bronze Lake
        new(16, new(1220, 1150)),                     // Camp Overlook
        new(55, new(1104, 1370)),                     // Wolves' Den Pier
 
        // --- The Black Shroud ---
        new(2, new(1564, 1104), UldPart: 3),          // New Gridania [Stadt]
        new(3, new(1530, 1150)),                      // Bentbranch Meadows
        new(4, new(1610, 1130)),                      // The Hawthorne Hut
        new(5, new(1590, 1180)),                      // Quarrymill
        new(6, new(1550, 1220)),                      // Camp Tranquil
        new(7, new(1500, 1100)),                      // Fallgourd Float
 
        // --- Thanalan ---
        new(9, new(1450, 1370), UldPart: 4),          // Ul'dah - Steps of Nald [Stadt]
        new(17, new(1380, 1330)),                     // Horizon
        new(53, new(1500, 1330)),                     // Black Brush Station
        new(18, new(1518, 1270)),                     // Camp Drybone
        new(19, new(1500, 1380)),                     // Little Ala Mhigo
        new(20, new(1472, 1426)),                     // Forgotten Springs
        new(21, new(1440, 1270)),                     // Camp Bluefog
        new(22, new(1420, 1240)),                     // Ceruleum Processing Plant
        new(62, new(1426, 1440)),                     // The Gold Saucer
 
        // --- Coerthas ---
        new(70, new(1400, 1040), UldPart: 7),         // Foundation [Stadt]
        new(23, new(1460, 1070)),                     // Camp Dragonhead
        new(71, new(1330, 1070)),                     // Falcon's Nest
 
        // --- Mor Dhona ---
        new(24, new(1440, 1150)),                     // Revenant's Toll
 
        // --- Abalathia's Spine ---
        new(72, new(1440, 920)),                      // Camp Cloudtop
        new(73, new(1426, 870)),                      // Ok' Zundu
        new(74, new(1460, 780)),                      // Helix
 
        // --- Dravania ---
        new(75, new(1160, 1012), UldPart: 14),        // Idyllshire [Stadt]
        new(76, new(1330, 1000)),                     // Tailfeather
        new(77, new(1270, 980)),                      // Anyx Trine
        new(78, new(1260, 920)),                      // Moghome
        new(79, new(1220, 890)),                      // Zenith
 
        // --- Gyr Abania ---
        new(104, new(1690, 1080), UldPart: 9),        // Rhalgr's Reach [Stadt]
        new(98, new(1640, 1090)),                     // Castrum Oriens
        new(99, new(1680, 1130)),                     // The Peering Stones
        new(100, new(1730, 1080)),                    // Ala Gannha
        new(101, new(1730, 1150)),                    // Ala Ghiri
        new(102, new(1748, 1120)),                    // Porta Praetoria
        new(103, new(1790, 1104)),                    // The Ala Mhigan Quarter
 
        // --- Hingashi / Othard ---
        new(111, new(3450, 1040), UldPart: 10),       // Kugane [Stadt]
        new(127, new(3250, 1050)),                    // The Doman Enclave [Stadt]
        new(105, new(3390, 1030)),                    // Tamamizu
        new(106, new(3380, 980)),                     // Onokoro
        new(107, new(3300, 1000)),                    // Namai
        new(108, new(3250, 980)),                     // The House of the Fierce
        new(109, new(3220, 850)),                     // Reunion
        new(110, new(3160, 800)),                     // The Dawn Throne
        new(128, new(3100, 820)),                     // Dhoro Iloh
 
        // --- The Northern Empty ---
        new(182, new(1040, 700), UldPart: 14),        // Old Sharlayan [Stadt]
        new(166, new(1010, 720)),                     // The Archeion
        new(167, new(980, 750)),                      // Sharlayan Hamlet
        new(168, new(950, 780)),                      // Aporia
 
        // --- Ilsabard ---
        new(183, new(2520, 1196), UldPart: 12),       // Radz-at-Han [Stadt]
        new(169, new(2470, 1280)),                    // Yedlihmad
        new(170, new(2430, 1260)),                    // The Great Work
        new(171, new(2484, 1242)),                    // Palaka's Stand
        new(172, new(2010, 700)),                     // Camp Broken Glass
        new(173, new(2024, 644)),                     // Tertium
 
        // --- Yok Tural ---
        new(216, new(350, 1130), UldPart: 21),        // Tuliyollal [Stadt]
        new(200, new(250, 1160)),                     // Wachunpelo
        new(201, new(260, 1220)),                     // Worlar's Echo
        new(202, new(320, 1310)),                     // Ok'hanu
        new(203, new(360, 1380)),                     // Many Fires
        new(204, new(300, 1390)),                     // Earthenshire
        new(238, new(368, 1330)),                     // Dock Poga
        new(205, new(480, 1350)),                     // Iq Br'aax
        new(206, new(552, 1440)),                     // Mamook
 
        // --- Xak Tural ---
        new(217, new(220, 920), UldPart: 22),         // Solution Nine [Stadt]
        new(207, new(360, 1070)),                     // Hhusatahwi
        new(208, new(320, 1050)),                     // Sheshenewezi Springs
        new(209, new(360, 1012)),                     // Mehwahhetsoan
        new(210, new(250, 1040)),                     // Yyasulani Station
        new(211, new(230, 980)),                      // The Outskirts
        new(212, new(184, 1012)),                     // Electrope Strike
 
        // --- Unlost World (Living Memory) ---
        new(213, new(2320, 2580)),                     // Leynode Mnemo
        new(214, new(2370, 2510)),                     // Leynode Pyro
        new(215, new(2280, 2500)),                     // Leynode Aero
 
        // ===================== Norvrandt =====================
 
        // --- Norvrandt ---
        new(133, new(585, 2800), UldPart: 34),         // The Crystarium [Stadt]
        new(134, new(120, 2900), UldPart: 35),         // Eulmore [Stadt]
        new(132, new(520, 2750)),                      // Fort Jobb
        new(136, new(420, 2740)),                      // The Ostall Imperative
        new(137, new(250, 2850)),                      // Stilltide
        new(138, new(170, 2880)),                      // Wright
        new(139, new(170, 2800)),                      // Tomra
        new(140, new(610, 2900)),                      // Mord Souq
        new(141, new(600, 3000)),                      // Twine
        new(161, new(500, 3000)),                      // The Inn at Journey's Head
        new(142, new(670, 2700)),                      // Slitherbough
        new(143, new(720, 2650)),                      // Fanow
        new(144, new(470, 2600)),                      // Lydha Lran
        new(145, new(460, 2500)),                      // Pla Enni
        new(146, new(550, 2500)),                      // Wolekdorf
        new(147, new(60, 2700)),                      // The Ondo Cups
        new(148, new(20, 2750)),                      // The Macarenses Angle
 
        // ===================== The Sea of Stars =====================
 
        // --- Mare Lamentorum ---
        new(174, new(1860, 2600)),                     // Sinus Lacrimarum
        new(175, new(1900, 2560)),                     // Bestways Burrow
 
        // --- Ultima Thule ---
        new(179, new(1990, 2530)),                     // Reah Tahra
        new(180, new(2024, 2470)),                     // Abode of the Ea
        new(181, new(2070, 2520)),                     // Base Omicron
 
        // ===================== The World Unsundered =====================
 
        // --- Elpis ---
        new(176, new(1600, 2550)),                     // Anagnorisis
        new(177, new(1545, 2600)),                     // The Twelve Wonders
        new(178, new(1550, 2510)),                     // Poieten Oikos

    ];
}
