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
        new(8, new(615, 1100), UldPart: 2),          // Limsa Lominsa Lower Decks [Stadt]
        new(52, new(640, 1100)),                     // Summerford Farms
        new(10, new(660, 1110)),                     // Moraby Drydocks
        new(11, new(690, 1080)),                     // Costa del Sol
        new(12, new(675, 1065)),                     // Wineport
        new(13, new(640, 1070)),                     // Swiftperch
        new(14, new(620, 1065)),                     // Aleport
        new(15, new(655, 1050)),                     // Camp Bronze Lake
        new(16, new(658, 1020)),                     // Camp Overlook
        new(55, new(600, 1140)),                     // Wolves' Den Pier
 
        // --- The Black Shroud ---
        new(2, new(850, 1000), UldPart: 3),          // New Gridania [Stadt]
        new(3, new(830, 1020)),                      // Bentbranch Meadows
        new(4, new(870, 1010)),                      // The Hawthorne Hut
        new(5, new(860, 1040)),                      // Quarrymill
        new(6, new(840, 1060)),                      // Camp Tranquil
        new(7, new(810, 995)),                       // Fallgourd Float
 
        // --- Thanalan ---
        new(9, new(785, 1140), UldPart: 4),          // Ul'dah - Steps of Nald [Stadt]
        new(17, new(750, 1120)),                     // Horizon
        new(53, new(810, 1120)),                     // Black Brush Station
        new(18, new(825, 1090)),                     // Camp Drybone
        new(19, new(810, 1150)),                     // Little Ala Mhigo
        new(20, new(800, 1175)),                     // Forgotten Springs
        new(21, new(780, 1090)),                     // Camp Bluefog
        new(22, new(770, 1070)),                     // Ceruleum Processing Plant
        new(62, new(775, 1180)),                     // The Gold Saucer
 
        // --- Coerthas ---
        new(70, new(760, 960), UldPart: 7),          // Foundation [Stadt]
        new(23, new(790, 980)),                      // Camp Dragonhead
        new(71, new(720, 980)),                      // Falcon's Nest
 
        // --- Mor Dhona ---
        new(24, new(780, 1025)),                     // Revenant's Toll
 
        // --- Abalathia's Spine ---
        new(72, new(780,  900)),                     // Camp Cloudtop
        new(73, new(775, 870)),                      // Ok' Zundu
        new(74, new(790, 820)),                      // Helix
 
        // --- Dravania ---
        new(75, new(630, 950), UldPart: 14),         // Idyllshire [Stadt]
        new(76, new(720, 940)),                      // Tailfeather
        new(77, new(690, 930)),                      // Anyx Trine
        new(78, new(680, 900)),                      // Moghome
        new(79, new(660, 880)),                      // Zenith
 
        // --- Gyr Abania ---
        new(104, new(915, 985), UldPart: 9),         // Rhalgr's Reach [Stadt]
        new(98, new(890, 990)),                      // Castrum Oriens
        new(99, new(910, 1010)),                     // The Peering Stones
        new(100, new(935, 985)),                     // Ala Gannha
        new(101, new(935, 1020)),                    // Ala Ghiri
        new(102, new(950, 1005)),                    // Porta Praetoria
        new(103, new(970, 1000)),                    // The Ala Mhigan Quarter
 
        // --- Hingashi / Othard ---
        new(111, new(1870, 960), UldPart: 10),       // Kugane [Stadt]
        new(127, new(1780, 970)),                    // The Doman Enclave [Stadt]
        new(105, new(1842, 955)),                    // Tamamizu
        new(106, new(1836, 930)),                    // Onokoro
        new(107, new(1810, 940)),                    // Namai
        new(108, new(1780, 930)),                    // The House of the Fierce
        new(109, new(1760, 860)),                    // Reunion
        new(110, new(1720, 840)),                    // The Dawn Throne
        new(128, new(1680, 845)),                    // Dhoro Iloh
 
        // --- The Northern Empty ---
        new(182, new(560, 780), UldPart: 14),        // Old Sharlayan [Stadt]
        new(166, new(545, 790)),                     // The Archeion
        new(167, new(530, 805)),                     // Sharlayan Hamlet
        new(168, new(515, 820)),                     // Aporia
 
        // --- Ilsabard ---
        new(183, new(1365, 1050), UldPart: 12),      // Radz-at-Han [Stadt]
        new(169, new(1340, 1095)),                   // Yedlihmad
        new(170, new(1320, 1080)),                   // The Great Work
        new(171, new(1350, 1075)),                   // Palaka's Stand
        new(172, new(1090, 780)),                    // Camp Broken Glass
        new(173, new(1100, 750)),                    // Tertium
 
        // --- Yok Tural ---
        new(216, new(185, 1010), UldPart: 21),       // Tuliyollal [Stadt]
        new(200, new(130, 1030)),                    // Wachunpelo
        new(201, new(140, 1060)),                    // Worlar's Echo
        new(202, new(170, 1110)),                    // Ok'hanu
        new(203, new(195, 1150)),                    // Many Fires
        new(204, new(160, 1155)),                    // Earthenshire
        new(238, new(200, 1120)),                    // Dock Poga
        new(205, new(260, 1130)),                    // Iq Br'aax
        new(206, new(300, 1180)),                    // Mamook
 
        // --- Xak Tural ---
        new(217, new(120, 900), UldPart: 22),        // Solution Nine [Stadt]
        new(207, new(195, 980)),                     // Hhusatahwi
        new(208, new(170, 970)),                     // Sheshenewezi Springs
        new(209, new(192, 950)),                     // Mehwahhetsoan
        new(210, new(130, 960)),                     // Yyasulani Station
        new(211, new(120, 930)),                     // The Outskirts
        new(212, new(100, 950)),                     // Electrope Strike
 
        // --- Unlost World (Living Memory) ---
        new(213, new(1240, 150)),                    // Leynode Mnemo
        new(214, new(1270, 110)),                    // Leynode Pyro
        new(215, new(1220, 105)),                    // Leynode Aero
 
        // ===================== Norvrandt =====================
 
        // --- Norvrandt ---
        new(133, new(535, 250), UldPart: 34),        // The Crystarium [Stadt]
        new(134, new(320, 320), UldPart: 35),        // Eulmore [Stadt]
        new(132, new(480, 240)),                     // Fort Jobb
        new(136, new(440, 220)),                     // The Ostall Imperative
        new(137, new(390, 290)),                     // Stilltide
        new(138, new(350, 290)),                     // Wright
        new(139, new(350, 260)),                     // Tomra
        new(140, new(530, 320)),                     // Mord Souq
        new(141, new(525, 350)),                     // Twine
        new(161, new(500, 330)),                     // The Inn at Journey's Head
        new(142, new(560, 190)),                     // Slitherbough
        new(143, new(590, 170)),                     // Fanow
        new(144, new(480, 180)),                     // Lydha Lran
        new(145, new(470, 130)),                     // Pla Enni
        new(146, new(510, 140)),                     // Wolekdorf
        new(147, new(310, 190)),                     // The Ondo Cups
        new(148, new(290, 210)),                     // The Macarenses Angle
 
        // ===================== The Sea of Stars =====================
 
        // --- Mare Lamentorum ---
        new(174, new(1010, 380)),                    // Sinus Lacrimarum
        new(175, new(1030, 360)),                    // Bestways Burrow
 
        // --- Ultima Thule ---
        new(179, new(1080, 340)),                    // Reah Tahra
        new(180, new(1100, 310)),                    // Abode of the Ea
        new(181, new(1120, 335)),                    // Base Omicron
 
        // ===================== The World Unsundered =====================
 
        // --- Elpis ---
        new(176, new(900, 125)),                     // Anagnorisis
        new(177, new(870, 150)),                     // The Twelve Wonders
        new(178, new(875, 100)),                     // Poieten Oikos


    ];
}
