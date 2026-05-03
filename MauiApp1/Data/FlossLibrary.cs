using System;
using System.Collections.Generic;
using System.Linq;
using MauiApp1.Models;

namespace MauiApp1.Data
{
    public static class FlossLibrary
    {
        public static readonly List<Floss> Colors = new List<Floss>
        {
            // === Page 1 ===
            new Floss { Red = 194, Green = 178, Blue = 128, ColorHex = "#C2B280", ColorName = "Кремовый", CodeGamma = "0001", CodeDmc = "613" },
            new Floss { Red = 151, Green = 84, Blue = 20, ColorHex = "#975414", ColorName = "Красного дерева оч.т.", CodeGamma = "0002", CodeDmc = "433" },
            new Floss { Red = 58, Green = 84, Blue = 103, ColorHex = "#3A5467", ColorName = "Синий оч.т.", CodeGamma = "0003", CodeDmc = "312,824,825" },
            new Floss { Red = 197, Green = 198, Blue = 190, ColorHex = "#C5C6BE", ColorName = "Стальной, св.", CodeGamma = "0004", CodeDmc = "318" },
            new Floss { Red = 115, Green = 140, Blue = 170, ColorHex = "#738CAA", ColorName = "Фарфоровый, ср.", CodeGamma = "0005", CodeDmc = "334" },

            new Floss { Red = 221, Green = 221, Blue = 218, ColorHex = "#DDDDDA", ColorName = "Жемчужно-серый", CodeGamma = "0006", CodeDmc = "168,415" },//168-У МЕНЯ НЕТ ТАКОГО В СПИСКЕ
            new Floss { Red = 187, Green = 107, Blue = 57, ColorHex = "#BB6B39", ColorName = "Коричневый, оч.св.", CodeGamma = "0007", CodeDmc = "435" },
            new Floss { Red = 176, Green = 187, Blue = 140, ColorHex = "#B0BB8C", ColorName = "Авокадо, оч.св.", CodeGamma = "0008", CodeDmc = "471" },
            new Floss { Red = 219, Green = 176, Blue = 122, ColorHex = "#DBB07A", ColorName = "Мокко, оч.св.", CodeGamma = "0009", CodeDmc = "613" },
            new Floss { Red = 206, Green = 155, Blue = 97, ColorHex = "#CE9B61", ColorName = "Оливково-зеленый, ср.", CodeGamma = "0010", CodeDmc = "471,733" },//471 УЖЕ БЫЛО (ПО 733)

            new Floss { Red = 255, Green = 0, Blue = 0, ColorHex = "#FF0000", ColorName = "Красно-оранжевый, яркий", CodeGamma = "0011", CodeDmc = "606" },
            new Floss { Red = 213, Green = 39, Blue = 86, ColorHex = "#D52756", ColorName = "Рождественский красный, яркий", CodeGamma = "0012", CodeDmc = "666" },
            new Floss { Red = 49, Green = 105, Blue = 85, ColorHex = "#316955", ColorName = "Изумрудный, ср.", CodeGamma = "0013", CodeDmc = "909" },
            new Floss { Red = 48, Green = 116, Blue = 91, ColorHex = "#30745B", ColorName = "Рождественский зеленый, яркий", CodeGamma = "0014", CodeDmc = "910" },
            new Floss { Red = 49, Green = 128, Blue = 97, ColorHex = "#318061", ColorName = "Изумрудный, яркий", CodeGamma = "0015", CodeDmc = "911" },

            new Floss { Red = 255, Green = 235, Blue = 168, ColorHex = "#FFEBA8", ColorName = "Топаз, оч.св.", CodeGamma = "0016", CodeDmc = "165,727" },////165-У МЕНЯ НЕТ ТАКОГО В СПИСКЕ
            new Floss { Red = 221, Green = 166, Blue = 107, ColorHex = "#DDA66B", ColorName = "Оливково-зеленый, св.", CodeGamma = "0017", CodeDmc = "734" },
            new Floss { Red = 206, Green = 175, Blue = 144, ColorHex = "#CEAF90", ColorName = "Желто-коричневый, оч. св.", CodeGamma = "0018", CodeDmc = "3782" },
            new Floss { Red = 255, Green = 183, Blue = 85, ColorHex = "#FFB755", ColorName = "Мандариновый, св.", CodeGamma = "0019", CodeDmc = "742" },
            new Floss { Red = 255, Green = 239, Blue = 170, ColorHex = "#FFEFAA", ColorName = "Желтый, бледный", CodeGamma = "0020", CodeDmc = "744" },

            new Floss { Red = 255, Green = 206, Blue = 158, ColorHex = "#FFCE9E", ColorName = "Желтый, бледный св.", CodeGamma = "0021", CodeDmc = "676,677" },
            new Floss { Red = 178, Green = 103, Blue = 70, ColorHex = "#B26746", ColorName = "Коричневый, св.", CodeGamma = "0022", CodeDmc = "434" },
            new Floss { Red = 124, Green = 135, Blue = 145, ColorHex = "#7C8791", ColorName = "Васильковый, св.", CodeGamma = "0023", CodeDmc = "931,932" },
            new Floss { Red = 220, Green = 61, Blue = 91, ColorHex = "#DC3D5B", ColorName = "Кораллово-красный, оч.т.", CodeGamma = "0024", CodeDmc = "349" },
            new Floss { Red = 255, Green = 220, Blue = 193, ColorHex = "#FFDCC1", ColorName = "Нежно розовый, св.", CodeGamma = "0025", CodeDmc = "3774" },

            new Floss { Red = 0, Green = 0, Blue = 73, ColorHex = "#000049", ColorName = "Синий, оч.т.", CodeGamma = "0026", CodeDmc = "823" },
            //new Floss { Red = 255, Green = 255, Blue = 255, ColorHex = "#FFFFFF", ColorName = "Фарфоровый, оч.св.", CodeGamma = "0027", CodeDmc = "384" },//такого нет в списке(нет 384)
            new Floss { Red = 138, Green = 24, Blue = 77, ColorHex = "#8A184D", ColorName = "Гранатовый оч.т.", CodeGamma = "0028", CodeDmc = "902" },
            new Floss { Red = 83, Green = 97, Blue = 73, ColorHex = "#536149", ColorName = "Зеленый попугаевый, оч.т.", CodeGamma = "0029", CodeDmc = "987" },
            new Floss { Red = 98, Green = 119, Blue = 57, ColorHex = "#627739", ColorName = "Зеленый попугаевый, т.", CodeGamma = "0030", CodeDmc = "905" },

            new Floss { Red = 219, Green = 55, Blue = 121, ColorHex = "#DB3779", ColorName = "Сливовый, ср.", CodeGamma = "0031", CodeDmc = "718" },
            new Floss { Red = 197, Green = 94, Blue = 88, ColorHex = "#C55E58", ColorName = "Медный, ср.", CodeGamma = "0032", CodeDmc = "920" },
            new Floss { Red = 128, Green = 152, Blue = 115, ColorHex = "#809873", ColorName = "Зеленый лесной", CodeGamma = "0033", CodeDmc = "3347" },
            new Floss { Red = 242, Green = 234, Blue = 219, ColorHex = "#F2EADB", ColorName = "Мокко, ультра светлый", CodeGamma = "0034", CodeDmc = "822,3033" },
            new Floss { Red = 85, Green = 73, Blue = 0, ColorHex = "#554900", ColorName = "Серо-зеленый, т.", CodeGamma = "0035", CodeDmc = "3051,3346" },

            new Floss { Red = 116, Green = 127, Blue = 96, ColorHex = "#747F60", ColorName = "Сосновый, ср.", CodeGamma = "0036", CodeDmc = "3363" },
            new Floss { Red = 255, Green = 199, Blue = 196, ColorHex = "#FFC7C4", ColorName = "Сливовый оч.св.", CodeGamma = "0037", CodeDmc = "3727" },
            new Floss { Red = 171, Green = 51, Blue = 87, ColorHex = "#AB3357", ColorName = "Розовато-лиловый, оч.т.", CodeGamma = "0038", CodeDmc = "3803" },//такого нет в списке(нет 3803)нашла в другой таблице
            new Floss { Red = 92, Green = 110, Blue = 108, ColorHex = "#5C6E6C", ColorName = "Серо-зеленый", CodeGamma = "0039", CodeDmc = "3768" },
            new Floss { Red = 239, Green = 214, Blue = 188, ColorHex = "#EFD6BC", ColorName = "Кремовый", CodeGamma = "0040", CodeDmc = "543" },
            new Floss { Red = 239, Green = 244, Blue = 164, ColorHex = "#EFF4A4", ColorName = "Лимонный, св.", CodeGamma = "0041", CodeDmc = "165" },//такого нет в списке(нет 165)нашла в другой таблице

            // === Page 2 ===
            new Floss { Red = 255, Green = 194, Blue = 67, ColorHex = "#FFC243", ColorName = "Канареечный, яркий", CodeGamma = "0042", CodeDmc = "973" },
            new Floss { Red = 255, Green = 253, Blue = 227, ColorHex = "#FFFDE3", ColorName = "Желтый, оч.св.", CodeGamma = "0043", CodeDmc = "3823" },//такого нет в списке(нет 3823)нашла в другой таблице
            new Floss { Red = 244, Green = 73, Blue = 0, ColorHex = "#F44900", ColorName = "Оранжевый, т.", CodeGamma = "0044", CodeDmc = "946" },
            new Floss { Red = 255, Green = 196, Blue = 184, ColorHex = "#FFC4B8", ColorName = "Абрикосовый, св.", CodeGamma = "0045", CodeDmc = "353,3824" },
            new Floss { Red = 237, Green = 134, Blue = 115, ColorHex = "#ED8673", ColorName = "Терракотовый, св.", CodeGamma = "0046", CodeDmc = "922" },

            new Floss { Red = 255, Green = 190, Blue = 164, ColorHex = "#FFBEA4", ColorName = "Красного дерева, оч.св.", CodeGamma = "0047", CodeDmc = "402,758,3827" },
            new Floss { Red = 173, Green = 114, Blue = 57, ColorHex = "#AD7239", ColorName = "Красного дерева, св.", CodeGamma = "0048", CodeDmc = "3826" },//такого нет в списке(нет 3826)нашла в другой таблице
            new Floss { Red = 246, Green = 141, Blue = 57, ColorHex = "#F68D39", ColorName = "Рыжий", CodeGamma = "0049", CodeDmc = "976" },
            new Floss { Red = 238, Green = 171, Blue = 121, ColorHex = "#EEAB79", ColorName = "Рыжий, св.", CodeGamma = "0050", CodeDmc = "437" },////////
            new Floss { Red = 251, Green = 227, Blue = 209, ColorHex = "#FBE3D1", ColorName = "Жемчужно-серый, оч.св.", CodeGamma = "0051", CodeDmc = "754,950" },

            new Floss { Red = 194, Green = 101, Blue = 76, ColorHex = "#C2654C", ColorName = "Пустынного песка", CodeGamma = "0052", CodeDmc = "407" },
            new Floss { Red = 210, Green = 185, Blue = 175, ColorHex = "#D2B9AF", ColorName = "Серо-фиолетовый", CodeGamma = "0053", CodeDmc = "452" },
           // new Floss { Red = 238, Green = 171, Blue = 121, ColorHex = "#EEAB79", ColorName = "Песочный", CodeGamma = "0054", CodeDmc = "437" },////////
            new Floss { Red = 123, Green = 71, Blue = 20, ColorHex = "#7B4714", ColorName = "Шоколадный", CodeGamma = "0055", CodeDmc = "801" },
            new Floss { Red = 203, Green = 182, Blue = 156, ColorHex = "#CBB69C", ColorName = "Бежевый т.", CodeGamma = "0056", CodeDmc = "3864" },//такого нет в списке(нет 3826)нашла в другой таблице

            new Floss { Red = 188, Green = 134, Blue = 107, ColorHex = "#BC866B", ColorName = "Серо-бежевый т.", CodeGamma = "0057", CodeDmc = "841" },
            new Floss { Red = 140, Green = 117, Blue = 109, ColorHex = "#8C756D", ColorName = "Серо-зеленый т.", CodeGamma = "0058", CodeDmc = "3790" },
            new Floss { Red = 174, Green = 176, Blue = 170, ColorHex = "#AEB0AA", ColorName = "Серо-зеленый", CodeGamma = "0059", CodeDmc = "642" },
            new Floss { Red = 121, Green = 121, Blue = 121, ColorHex = "#797979", ColorName = "Серо-зеленый, т.", CodeGamma = "0060", CodeDmc = "646" },
            new Floss { Red = 167, Green = 139, Blue = 136, ColorHex = "#A78B88", ColorName = "Стальной, т.", CodeGamma = "0061", CodeDmc = "414" },

            new Floss { Red = 194, Green = 36, Blue = 67, ColorHex = "#C22443", ColorName = "Лососевый, оч.т.", CodeGamma = "0062", CodeDmc = "347" },
            new Floss { Red = 255, Green = 85, Blue = 91, ColorHex = "#FF555B", ColorName = "Рождественский красный, св.", CodeGamma = "0063", CodeDmc = "3705,3801" },
            new Floss { Red = 188, Green = 22, Blue = 65, ColorHex = "#BC1641", ColorName = "Красный, т.", CodeGamma = "0064", CodeDmc = "326" },
            new Floss { Red = 255, Green = 154, Blue = 148, ColorHex = "#FF9A94", ColorName = "Пыльной розы", CodeGamma = "0065", CodeDmc = "3733" },
            new Floss { Red = 255, Green = 157, Blue = 150, ColorHex = "#FF9D96", ColorName = "Розы, св.", CodeGamma = "0066", CodeDmc = "3326,3354" },

            new Floss { Red = 225, Green = 205, Blue = 200, ColorHex = "#E1CDC8", ColorName = "Серо-сиреневый", CodeGamma = "0067", CodeDmc = "3042" },
            new Floss { Red = 186, Green = 145, Blue = 170, ColorHex = "#BA91AA", ColorName = "Серо-сиреневый, св.", CodeGamma = "0068", CodeDmc = "3836" },//такого нет в списке(нет 3836)нашла в другой таблице
            new Floss { Red = 243, Green = 71, Blue = 139, ColorHex = "#F3478B", ColorName = "Фуксия", CodeGamma = "0069", CodeDmc = "3805" },//такого нет в списке(нет 3805)нашла в другой таблице
            new Floss { Red = 255, Green = 140, Blue = 174, ColorHex = "#FF8CAE", ColorName = "Клюквенный, св.", CodeGamma = "0070", CodeDmc = "3806" },//такого нет в списке(нет 3806)нашла в другой таблице
            new Floss { Red = 231, Green = 84, Blue = 122, ColorHex = "#E7547A", ColorName = "Клюквенный", CodeGamma = "0071", CodeDmc = "602" },

            new Floss { Red = 231, Green = 79, Blue = 134, ColorHex = "#E74F86", ColorName = "Сливовый, св.", CodeGamma = "0072", CodeDmc = "3607" },
            new Floss { Red = 171, Green = 22, Blue = 95, ColorHex = "#AB165F", ColorName = "Сливовый", CodeGamma = "0073", CodeDmc = "718,917" },//917-ВЫБРАН, Т.К. 718 УЖЕ БЫЛ
            new Floss { Red = 160, Green = 100, Blue = 146, ColorHex = "#A06492", ColorName = "Фиолетовый", CodeGamma = "0074", CodeDmc = "553" },
            new Floss { Red = 109, Green = 18, Blue = 97, ColorHex = "#6D1261", ColorName = "Фиолетовый, оч.т.", CodeGamma = "0075", CodeDmc = "550" },
            new Floss { Red = 236, Green = 207, Blue = 225, ColorHex = "#ECCFE1", ColorName = "Сиреневый, св.", CodeGamma = "0076", CodeDmc = "210" },

            new Floss { Red = 146, Green = 85, Blue = 130, ColorHex = "#925582", ColorName = "Фиолетовый", CodeGamma = "0077", CodeDmc = "552" },
            new Floss { Red = 162, Green = 121, Blue = 164, ColorHex = "#A279A4", ColorName = "Сине-фиолетовый", CodeGamma = "0078", CodeDmc = "340" },
            new Floss { Red = 127, Green = 84, Blue = 130, ColorHex = "#7F5482", ColorName = "Сине-фиолетовый, т.", CodeGamma = "0079", CodeDmc = "333" },
            new Floss { Red = 92, Green = 114, Blue = 148, ColorHex = "#5С7294", ColorName = "Васильковый, св.", CodeGamma = "0080", CodeDmc = "3838" },//такого нет в списке(нет 3838)нашла в другой таблице
            new Floss { Red = 187, Green = 208, Blue = 218, ColorHex = "#BBD0DA", ColorName = "Лавандово-синий", CodeGamma = "0081", CodeDmc = "794" },
            new Floss { Red = 103, Green = 115, Blue = 141, ColorHex = "#67738D", ColorName = "Васильковый", CodeGamma = "0082", CodeDmc = "798" },
            new Floss { Red = 30, Green = 66, Blue = 99, ColorHex = "#1E4263", ColorName = "Синий, т.", CodeGamma = "0083", CodeDmc = "797" },
            new Floss { Red = 115, Green = 138, Blue = 153, ColorHex = "#738A99", ColorName = "Синий", CodeGamma = "0084", CodeDmc = "826" },

            // === Page 3 ===
            new Floss { Red = 18, Green = 174, Blue = 186, ColorHex = "#12AEBA", ColorName = "Бирюзовый, яркий т.", CodeGamma = "0085", CodeDmc = "3844" },//такого нет в списке(нет 3844)нашла в другой таблице
            new Floss { Red = 20, Green = 170, Blue = 208, ColorHex = "#14AAD0", ColorName = "Электрик синий", CodeGamma = "0086", CodeDmc = "3843" },//такого нет в списке(нет 3843)нашла в другой таблице
            new Floss { Red = 4, Green = 196, Blue = 202, ColorHex = "#04C4CA", ColorName = "Бирюзовый, яркий т.", CodeGamma = "0087", CodeDmc = "3845" },//такого нет в списке(нет 3845)нашла в другой таблице
            //new Floss { Red = 48, Green = 116, Blue = 91, ColorHex = "#30745B", ColorName = "Изумрудный", CodeGamma = "0088", CodeDmc = "910" },//УЖЕ БЫЛО
            new Floss { Red = 95, Green = 112, Blue = 91, ColorHex = "#5F705B", ColorName = "Фисташково-зеленый", CodeGamma = "0089", CodeDmc = "367" },

            new Floss { Red = 116, Green = 114, Blue = 92, ColorHex = "#74725C", ColorName = "Авокадо", CodeGamma = "0090", CodeDmc = "469,470" },
            new Floss { Red = 187, Green = 179, Blue = 148, ColorHex = "#BBB394", ColorName = "Серо-зеленый, св.", CodeGamma = "0091", CodeDmc = "3053" },
            new Floss { Red = 184, Green = 138, Blue = 87, ColorHex = "#B88A57", ColorName = "Горчичный", CodeGamma = "0092", CodeDmc = "370" },
            new Floss { Red = 229, Green = 193, Blue = 139, ColorHex = "#E5C18B", ColorName = "Песочный", CodeGamma = "0093", CodeDmc = "3046" },
            new Floss { Red = 136, Green = 95, Blue = 18, ColorHex = "#885F12", ColorName = "Табачный", CodeGamma = "0094", CodeDmc = "830,831" },

            //new Floss { Red = 255, Green = 231, Blue = 182, ColorHex = "#FFE7B6", ColorName = "Кремовый, св.", CodeGamma = "0095", CodeDmc = "677" },//
            //new Floss { Red = 255, Green = 231, Blue = 182, ColorHex = "#FFE7B6", ColorName = "Бежевый, св.", CodeGamma = "0096", CodeDmc = "677" },
            new Floss { Red = 225, Green = 146, Blue = 85, ColorHex = "#E19255", ColorName = "Золотисто-коричневый", CodeGamma = "0097", CodeDmc = "783" },
            new Floss { Red = 237, Green = 172, Blue = 123, ColorHex = "#EDAC7B", ColorName = "Бежевый", CodeGamma = "0098", CodeDmc = "422,3046" },
          //  new Floss { Red = 255, Green = 231, Blue = 182, ColorHex = "#FFE7B6", ColorName = "", CodeGamma = "0099", CodeDmc = "676,3046" },

            new Floss { Red = 255, Green = 255, Blue = 220, ColorHex = "#FFFFDC", ColorName = "Желтый, оч.св.", CodeGamma = "0101", CodeDmc = "3078" },
            //new Floss { Red = 255, Green = 235, Blue = 168, ColorHex = "#FFEBA8", ColorName = "Желтый, св.", CodeGamma = "0102", CodeDmc = "727,3078" },
            new Floss { Red = 255, Green = 230, Blue = 146, ColorHex = "#FFE692", ColorName = "Желтый", CodeGamma = "0104", CodeDmc = "743" },
            new Floss { Red = 255, Green = 146, Blue = 0, ColorHex = "#FF9200", ColorName = "Мандариновый", CodeGamma = "0105", CodeDmc = "972" },
            new Floss { Red = 242, Green = 151, Blue = 70, ColorHex = "#F29746", ColorName = "Оранжевый", CodeGamma = "0107", CodeDmc = "3853" },

           // new Floss { Red = 255, Green = 106, Blue = 0, ColorHex = "#FF6A00", ColorName = "Рыже-оранжевый", CodeGamma = "0108", CodeDmc = "3853" },
            new Floss { Red = 255, Green = 115, Blue = 97, ColorHex = "#FF7361", ColorName = "Морковный", CodeGamma = "0109", CodeDmc = "721" },
            new Floss { Red = 206, Green = 148, Blue = 186, ColorHex = "#CE94BA", ColorName = "Сиреневый, т.", CodeGamma = "0111", CodeDmc = "208,209" },
            new Floss { Red = 108, Green = 58, Blue = 110, ColorHex = "#6C3A6E", ColorName = "Сиреневый, оч.т.", CodeGamma = "0112", CodeDmc = "3837" },
            new Floss { Red = 255, Green = 211, Blue = 212, ColorHex = "#FFD3D4", ColorName = "Розовый, св.", CodeGamma = "0114", CodeDmc = "3716" },

            new Floss { Red = 255, Green = 109, Blue = 115, ColorHex = "#FF6D73", ColorName = "Розовый, яркий", CodeGamma = "0116", CodeDmc = "602,956" },
            new Floss { Red = 241, Green = 49, Blue = 84, ColorHex = "#F13154", ColorName = "Розово-красный", CodeGamma = "0118", CodeDmc = "891" },
            //new Floss { Red = 220, Green = 61, Blue = 91, ColorHex = "#DC3D5B", ColorName = "Рождественский красный, яркий", CodeGamma = "0119", CodeDmc = "349" },
            new Floss { Red = 231, Green = 18, Blue = 97, ColorHex = "#E71261", ColorName = "Рождественский красный", CodeGamma = "0120", CodeDmc = "321" },
            new Floss { Red = 198, Green = 190, Blue = 173, ColorHex = "#C6BEAD", ColorName = "Серый, св.", CodeGamma = "0150", CodeDmc = "3023" },

            //new Floss { Red = 121, Green = 121, Blue = 121, ColorHex = "#797979", ColorName = "Серый", CodeGamma = "0152", CodeDmc = "646" },
            new Floss { Red = 67, Green = 63, Blue = 47, ColorHex = "#433F2F", ColorName = "Серый, т.", CodeGamma = "0154", CodeDmc = "935" },
           // new Floss { Red = 121, Green = 121, Blue = 121, ColorHex = "#797979", ColorName = "Серо-зеленый, оч.т.", CodeGamma = "0155", CodeDmc = "935" },
           // new Floss { Red = 255, Green = 255, Blue = 255, ColorHex = "#FFFFFF", ColorName = "Пыльной розы, оч.св.", CodeGamma = "0201", CodeDmc = "963" },
            new Floss { Red = 255, Green = 189, Blue = 202, ColorHex = "#FFBDCA", ColorName = "Розовый", CodeGamma = "0202", CodeDmc = "604" },

           // new Floss { Red = 255, Green = 146, Blue = 0, ColorHex = "#FF9200", ColorName = "Розовый, яркий", CodeGamma = "0204", CodeDmc = "3806" },
            new Floss { Red = 205, Green = 217, Blue = 154, ColorHex = "#CDD99A", ColorName = "Желтый", CodeGamma = "0206", CodeDmc = "12" },
            new Floss { Red = 165, Green = 164, Blue = 103, ColorHex = "#A5A467", ColorName = "Салатовый", CodeGamma = "0207", CodeDmc = "704" },
            new Floss { Red = 121, Green = 144, Blue = 76, ColorHex = "#79904C", ColorName = "Зеленый, яркий", CodeGamma = "0209", CodeDmc = "703" },
            new Floss { Red = 79, Green = 121, Blue = 66, ColorHex = "#4F7942", ColorName = "Травяной", CodeGamma = "0210", CodeDmc = "702" },
            new Floss { Red = 51, Green = 131, Blue = 98, ColorHex = "#338362", ColorName = "Зеленый, т.", CodeGamma = "0212", CodeDmc = "505" },

            // === Page 4 ===
            new Floss { Red = 243, Green = 176, Blue = 128, ColorHex = "#F3B080", ColorName = "Охра", CodeGamma = "0214", CodeDmc = "729,783" },
            new Floss { Red = 169, Green = 130, Blue = 4, ColorHex = "#A98204", ColorName = "Охра, т.", CodeGamma = "0215", CodeDmc = "3829" },
           // new Floss { Red = 178, Green = 103, Blue = 70, ColorHex = "#B26746", ColorName = "Охра, оч.т.", CodeGamma = "0216", CodeDmc = "434" },
            new Floss { Red = 138, Green = 110, Blue = 78, ColorHex = "#8A6E4E", ColorName = "Коричневый, св.", CodeGamma = "0217", CodeDmc = "3862" },
           // new Floss { Red = 123, Green = 71, Blue = 20, ColorHex = "#7B4714", ColorName = "Коричневый", CodeGamma = "0219", CodeDmc = "801,3882" },

            new Floss { Red = 94, Green = 56, Blue = 27, ColorHex = "#5E381B", ColorName = "Коричневый, т.", CodeGamma = "0220", CodeDmc = "838" },
            new Floss { Red = 247, Green = 246, Blue = 248, ColorHex = "#F7F6F8", ColorName = "Голубой, оч.св.", CodeGamma = "0301", CodeDmc = "775" },
            new Floss { Red = 176, Green = 192, Blue = 218, ColorHex = "#B0C0DA", ColorName = "Голубой", CodeGamma = "0302", CodeDmc = "3840" },
            new Floss { Red = 81, Green = 109, Blue = 135, ColorHex = "#516D87", ColorName = "Синий, св.", CodeGamma = "0304", CodeDmc = "322" },
            new Floss { Red = 71, Green = 97, Blue = 116, ColorHex = "#476174", ColorName = "Синий", CodeGamma = "0306", CodeDmc = "824,3885" },

            new Floss { Red = 71, Green = 55, Blue = 93, ColorHex = "#47375D", ColorName = "Синий оч.т.", CodeGamma = "0308", CodeDmc = "791" },
            new Floss { Red = 248, Green = 247, Blue = 221, ColorHex = "#F8F7DD", ColorName = "Персиковый, оч.св.", CodeGamma = "0309", CodeDmc = "819" },
           // new Floss { Red = 255, Green = 196, Blue = 184, ColorHex = "#FFC4B8", ColorName = "Персиковый, св.", CodeGamma = "0310", CodeDmc = "353" },
            new Floss { Red = 255, Green = 157, Blue = 144, ColorHex = "#FF9D90", ColorName = "Персиковый", CodeGamma = "0312", CodeDmc = "352" },
            new Floss { Red = 255, Green = 123, Blue = 103, ColorHex = "#FF7B67", ColorName = "Коралловый", CodeGamma = "0314", CodeDmc = "3340" },

            new Floss { Red = 255, Green = 128, Blue = 135, ColorHex = "#FF8087", ColorName = "Красно-оранжевый", CodeGamma = "0315", CodeDmc = "351,3340" },
           // new Floss { Red = 121, Green = 144, Blue = 76, ColorHex = "#79904C", ColorName = "Зеленый, т.", CodeGamma = "0316", CodeDmc = "703" },
            //new Floss { Red = 79, Green = 121, Blue = 66, ColorHex = "#4F7942", ColorName = "Рождественский зеленый, оч.св.", CodeGamma = "0319", CodeDmc = "702" },
            new Floss { Red = 0, Green = 96, Blue = 47, ColorHex = "#00602F", ColorName = "Рождественский зеленый, св.", CodeGamma = "0321", CodeDmc = "700" },
            //new Floss { Red = 255, Green = 146, Blue = 0, ColorHex = "#FF9200", ColorName = "Розового цикламена", CodeGamma = "0402", CodeDmc = "3806" },

            new Floss { Red = 208, Green = 57, Blue = 106, ColorHex = "#D0396A", ColorName = "Малиновый", CodeGamma = "0404", CodeDmc = "600" },
            new Floss { Red = 145, Green = 53, Blue = 70, ColorHex = "#913546", ColorName = "Вишневый", CodeGamma = "0406", CodeDmc = "777" },
           // new Floss { Red = 179, Green = 0, Blue = 91, ColorHex = "#B3005B", ColorName = "Гранатовый", CodeGamma = "0407", CodeDmc = "777" },
            new Floss { Red = 162, Green = 0, Blue = 88, ColorHex = "#A6005B", ColorName = "Гранатовый, т.", CodeGamma = "0409", CodeDmc = "814" },
            new Floss { Red = 166, Green = 0, Blue = 91, ColorHex = "#A6005B", ColorName = "Гранатовый, оч.т.", CodeGamma = "0410", CodeDmc = "815" },

            new Floss { Red = 138, Green = 153, Blue = 120, ColorHex = "#8A9978", ColorName = "Зеленый, св.", CodeGamma = "0411", CodeDmc = "320" },
            new Floss { Red = 17, Green = 90, Blue = 56, ColorHex = "#115A3B", ColorName = "Изумрудный, т.", CodeGamma = "0414", CodeDmc = "909,3818" },
           // new Floss { Red = 255, Green = 255, Blue = 255, ColorHex = "#FFFFFF", ColorName = "Белый", CodeGamma = "0415", CodeDmc = "B5200" },
           // new Floss { Red = 255, Green = 255, Blue = 255, ColorHex = "#FFFFFF", ColorName = "Фиолетово-синий, св.", CodeGamma = "0416", CodeDmc = "-" },
            //new Floss { Red = 225, Green = 205, Blue = 200, ColorHex = "#E1CDC8", ColorName = "Фиолетово-синий", CodeGamma = "0417", CodeDmc = "3042" },

            new Floss { Red = 103, Green = 64, Blue = 118, ColorHex = "#674076", ColorName = "Фиолетово-синий, т.", CodeGamma = "0419", CodeDmc = "29" },
            new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Черный", CodeGamma = "0420", CodeDmc = "310" },
            new Floss { Red = 237, Green = 254, Blue = 217, ColorHex = "#EDFED9", ColorName = "Желто-зеленый, оч.св.", CodeGamma = "0500", CodeDmc = "10" },
            new Floss { Red = 208, Green = 251, Blue = 178, ColorHex = "#D0FBB2", ColorName = "Салатовый, св.", CodeGamma = "0501", CodeDmc = "14" },
            //new Floss { Red = 255, Green = 255, Blue = 255, ColorHex = "#FFFFFF", ColorName = "Салатовый", CodeGamma = "0502", CodeDmc = "-" },

           // new Floss { Red = 49, Green = 128, Blue = 97, ColorHex = "#318061", ColorName = "Изумрудный", CodeGamma = "0504", CodeDmc = "911" },
            new Floss { Red = 47, Green = 91, Blue = 73, ColorHex = "#2F5B49", ColorName = "Аквамарин", CodeGamma = "0506", CodeDmc = "991" },
            new Floss { Red = 16, Green = 127, Blue = 135, ColorHex = "#107F87", ColorName = "Синий, св.", CodeGamma = "0512", CodeDmc = "517" },
            new Floss { Red = 102, Green = 142, Blue = 152, ColorHex = "#668E98", ColorName = "Синий", CodeGamma = "0513", CodeDmc = "3760" },
            //new Floss { Red = 255, Green = 255, Blue = 255, ColorHex = "#FFFFFF", ColorName = "Лимонный, св.", CodeGamma = "0514", CodeDmc = "3889" },

           // new Floss { Red = 255, Green = 194, Blue = 67, ColorHex = "#FFC243", ColorName = "Желтый", CodeGamma = "0516", CodeDmc = "973" },
            new Floss { Red = 171, Green = 206, Blue = 177, ColorHex = "#ABCEB1", ColorName = "Морской воды", CodeGamma = "0605", CodeDmc = "959" },
            new Floss { Red = 0, Green = 160, Blue = 130, ColorHex = "#00A082", ColorName = "Морской воды, т.", CodeGamma = "0606", CodeDmc = "958" },
          //  new Floss { Red = 255, Green = 255, Blue = 255, ColorHex = "#FFFFFF", ColorName = "Сливовый, оч.св.", CodeGamma = "0609", CodeDmc = "3836" },

            //// === Page 5 ===
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Сливовый, св.", CodeGamma = "0611", CodeDmc = "718" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Сливковый, св.", CodeGamma = "0613", CodeDmc = "989" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Оливковый", CodeGamma = "0614", CodeDmc = "470" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Хаки, оч.т.", CodeGamma = "0616", CodeDmc = "3345" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Серых ракушек", CodeGamma = "0644", CodeDmc = "642" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Оливково-зеленый, оч.св.", CodeGamma = "0652", CodeDmc = "165" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Оливково-зеленый, св.", CodeGamma = "0653", CodeDmc = "734" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Оливково-золотой", CodeGamma = "0654", CodeDmc = "832" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Оливково-золотой, т.", CodeGamma = "0655", CodeDmc = "831" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Кофейный, т.", CodeGamma = "0657", CodeDmc = "3781" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Серо-зеленый, оч.св.", CodeGamma = "0661", CodeDmc = "928" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Серо-зеленый, св.", CodeGamma = "0662", CodeDmc = "927" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Серо-зеленый", CodeGamma = "0663", CodeDmc = "927" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Серо-зеленый, т.", CodeGamma = "0664", CodeDmc = "3768" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Серо-зеленый, оч.т.", CodeGamma = "0665", CodeDmc = "413" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Серо-зеленый, оч.оч.т.", CodeGamma = "0666", CodeDmc = "924" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Серых ракушек, оч.св.", CodeGamma = "0670", CodeDmc = "3033" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Серых ракушек, св.", CodeGamma = "0672", CodeDmc = "07" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Серых ракушек", CodeGamma = "0673", CodeDmc = "07" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Серых ракушек, т.", CodeGamma = "0674", CodeDmc = "844" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Серых ракушек, оч.т.", CodeGamma = "0676", CodeDmc = "844" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Персиковый", CodeGamma = "0703", CodeDmc = "754" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Кораллово-красный, св.", CodeGamma = "0705", CodeDmc = "21" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Кораллово-красный", CodeGamma = "0707", CodeDmc = "351" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Кораллово-красный, оч.т.", CodeGamma = "0708", CodeDmc = "347" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Рождественский красный", CodeGamma = "0709", CodeDmc = "816" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Фисташково-зеленый, оч.св.", CodeGamma = "0715", CodeDmc = "368" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Фисташково-зеленый, св.", CodeGamma = "0716", CodeDmc = "368" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Зеленый лесной", CodeGamma = "0717", CodeDmc = "987" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Зеленый лесной, т.", CodeGamma = "0718", CodeDmc = "987" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Фисташково-зеленый, оч.т.", CodeGamma = "0719", CodeDmc = "895" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Зеленый, оч.оч.т.", CodeGamma = "0720", CodeDmc = "895" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Сиреневый, св.", CodeGamma = "0725", CodeDmc = "453,3743" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Сиреневый", CodeGamma = "0727", CodeDmc = "554" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Фиолетовый, т.", CodeGamma = "0729", CodeDmc = "-" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Нежно голубой, оч.св.", CodeGamma = "0752", CodeDmc = "928" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Нежно голубой, св.", CodeGamma = "0753", CodeDmc = "168,3753" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Античный синий, св.", CodeGamma = "0754", CodeDmc = "318" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Античный синий", CodeGamma = "0755", CodeDmc = "932" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Васильковый, св.", CodeGamma = "0756", CodeDmc = "794" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Синий, св.", CodeGamma = "0757", CodeDmc = "322" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Серо-синий", CodeGamma = "0758", CodeDmc = "-" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Синий яркий", CodeGamma = "0759", CodeDmc = "-" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Розово-бежевый, св.", CodeGamma = "0765", CodeDmc = "225" },

            //// === Page 6 ===
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Терракотовый", CodeGamma = "0768", CodeDmc = "3772,3778" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Красной меди, т.", CodeGamma = "0771", CodeDmc = "918" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Розового дерева, т.", CodeGamma = "0772", CodeDmc = "3857,3858" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Желто-коричневый, оч.св.", CodeGamma = "0802", CodeDmc = "739" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Желто-коричневый, св.", CodeGamma = "0803", CodeDmc = "738" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Желто-коричневый", CodeGamma = "0804", CodeDmc = "422" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Желто-коричневый, т.", CodeGamma = "0805", CodeDmc = "167" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Желто-коричневый, оч.т.", CodeGamma = "0806", CodeDmc = "167" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Желто-коричневый, оч.т.", CodeGamma = "0807", CodeDmc = "167" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Коричневый, св.", CodeGamma = "0808", CodeDmc = "3862" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Коричневый", CodeGamma = "0809", CodeDmc = "433,869" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Кофейный, т.", CodeGamma = "0811", CodeDmc = "938" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Кофейный, оч.т.", CodeGamma = "0812", CodeDmc = "838" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Авокадо, оч.св.", CodeGamma = "0815", CodeDmc = "165" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Авокадо, оч.св.", CodeGamma = "0816", CodeDmc = "11" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Авокадо, св.", CodeGamma = "0817", CodeDmc = "472" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Авокадо", CodeGamma = "0818", CodeDmc = "581" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Авокадо, т.", CodeGamma = "0820", CodeDmc = "580" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Авокадо, оч.т.", CodeGamma = "0821", CodeDmc = "937" },
            //new Floss { Red = 0, Green = 34, Blue = 0, ColorHex = "#002200", ColorName = "Авокадо, оч.оч.т.", CodeGamma = "0823", CodeDmc = "3051" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Небесно-голубой, оч.св.", CodeGamma = "0852", CodeDmc = "747,3811" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Бирюзовый, св.", CodeGamma = "0853", CodeDmc = "3811" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Бирюзовый", CodeGamma = "0854", CodeDmc = "598" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Бирюзовый, т.", CodeGamma = "0855", CodeDmc = "3766" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Морская волна, св.", CodeGamma = "0856", CodeDmc = "3810" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Морская волна", CodeGamma = "0857", CodeDmc = "3809" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Морская волна, т.", CodeGamma = "0858", CodeDmc = "3808" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Морская волна, оч.т.", CodeGamma = "0859", CodeDmc = "3808" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Морская волна, оч.оч.т.", CodeGamma = "0860", CodeDmc = "3808" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Гранатовый, св.", CodeGamma = "0865", CodeDmc = "221,3721" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Гранатовый, св.", CodeGamma = "0867", CodeDmc = "816" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Гранатовый", CodeGamma = "0869", CodeDmc = "815" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Гранатовый, т.", CodeGamma = "0870", CodeDmc = "814" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Гранатовый, оч.т.", CodeGamma = "0871", CodeDmc = "154,814" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Грязно розовый, св.", CodeGamma = "0875", CodeDmc = "778" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Грязно розовый", CodeGamma = "0876", CodeDmc = "152" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Грязно розовый, оч.т.", CodeGamma = "0877", CodeDmc = "152" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Грязно розовый, оч.оч.т.", CodeGamma = "0879", CodeDmc = "3726" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Нежно розовый", CodeGamma = "0902", CodeDmc = "225" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Нежно розовый, т.", CodeGamma = "0904", CodeDmc = "3354" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Розовато-лиловый, св.", CodeGamma = "0905", CodeDmc = "3688" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Розовато-лиловый", CodeGamma = "0906", CodeDmc = "3687" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Розовато-лиловый, т.", CodeGamma = "0907", CodeDmc = "718,917" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Розовато-лиловый, оч. т.", CodeGamma = "0908", CodeDmc = "3685" },

            //// === Page 7 ===
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Красного винограда, оч.т.", CodeGamma = "0909", CodeDmc = "35,3685" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Мокко, св.", CodeGamma = "0915", CodeDmc = "642" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Тускло-коричневый", CodeGamma = "0917", CodeDmc = "640" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Тускло-коричневый, т.", CodeGamma = "0919", CodeDmc = "3787,3790" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Рыжевато-коричневый", CodeGamma = "0924", CodeDmc = "437" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Пустынного песка", CodeGamma = "0925", CodeDmc = "436" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Золотовато-коричневый, св.", CodeGamma = "0926", CodeDmc = "434,435" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Золотовато-коричневый", CodeGamma = "0927", CodeDmc = "434,435" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Золотовато-коричневый, т.", CodeGamma = "0928", CodeDmc = "433" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Коричневый", CodeGamma = "0929", CodeDmc = "801" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Красного дерева, оч.т.", CodeGamma = "0930", CodeDmc = "938" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Сине-зеленый, оч.св.", CodeGamma = "0953", CodeDmc = "3813" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Сине-зеленый", CodeGamma = "0954", CodeDmc = "3817" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Серовато-зеленый", CodeGamma = "0956", CodeDmc = "163" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Серовато-зеленый, т.", CodeGamma = "0957", CodeDmc = "561" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Нефрит, оч.т.", CodeGamma = "0958", CodeDmc = "319" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Зеленый, оч.т.", CodeGamma = "0960", CodeDmc = "319,500" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", Blank = "", CodeGamma = "0966", CodeDmc = "302" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Жемчужно-серый", CodeGamma = "0967", CodeDmc = "168" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Жемчужно-серый, т.", CodeGamma = "0969", CodeDmc = "169" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Античный синий", CodeGamma = "0970", CodeDmc = "3768" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Античный синий, т.", CodeGamma = "0971", CodeDmc = "3768" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Античный синий, оч.т.", CodeGamma = "0972", CodeDmc = "930" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Лаванда, св.", CodeGamma = "1095", CodeDmc = "211" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Старого золота, св.", CodeGamma = "2125", CodeDmc = "738" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Старого золота", CodeGamma = "2130", CodeDmc = "3045" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Топаз, оч.т.", CodeGamma = "2135", CodeDmc = "729" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Лососевый, оч.св.", CodeGamma = "3010", CodeDmc = "950" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Лососевый, св.", CodeGamma = "3011", CodeDmc = "224" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Лососевый", CodeGamma = "3012", CodeDmc = "760" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Грязно розовый, св.", CodeGamma = "3013", CodeDmc = "224" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Грязно розовый", CodeGamma = "3014", CodeDmc = "223" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Грязно розовый, т.", CodeGamma = "3015", CodeDmc = "223" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Розово-лиловый, св.", CodeGamma = "3016", CodeDmc = "778" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Сливовый", CodeGamma = "3017", CodeDmc = "3041" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Желтовато-бежевый, т.", CodeGamma = "3018", CodeDmc = "3045" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Старого золота", CodeGamma = "3019", CodeDmc = "680" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Соломенный", CodeGamma = "3020", CodeDmc = "3820" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Кремовый, св.", CodeGamma = "3021", CodeDmc = "738,739,945" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Красного дерева", CodeGamma = "3022", CodeDmc = "301" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Красного дерева, т.", CodeGamma = "3023", CodeDmc = "301" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Персиковый, св.", CodeGamma = "3024", CodeDmc = "3774" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Персиковый", CodeGamma = "3025", CodeDmc = "758" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Персиковый, т.", CodeGamma = "3026", CodeDmc = "3771" },

            //// === Page 8 ===
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Терракотовый", CodeGamma = "3027", CodeDmc = "356" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Розового дерева, св.", CodeGamma = "3028", CodeDmc = "3859" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Розового дерева", CodeGamma = "3029", CodeDmc = "3858" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Пустынного песка, св.", CodeGamma = "3030", CodeDmc = "-" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Терракотовый, св.", CodeGamma = "3031", CodeDmc = "407,3859" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Какао", CodeGamma = "3032", CodeDmc = "3860" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Бежево-коричневый, оч.св.", CodeGamma = "3033", CodeDmc = "543,3893" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Бежево-коричневый", CodeGamma = "3034", CodeDmc = "841" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Бежево-коричневый, т.", CodeGamma = "3035", CodeDmc = "3863" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Кремовый", CodeGamma = "3036", CodeDmc = "822,3033" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Бежево-серый, оч.т.", CodeGamma = "3037", CodeDmc = "640" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Коричнево-серый, оч.св.", CodeGamma = "3038", CodeDmc = "644" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Коричнево-серый, св.", CodeGamma = "3039", CodeDmc = "3023" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Коричнево-серый", CodeGamma = "3040", CodeDmc = "640,642" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Мокко, св.", CodeGamma = "3041", CodeDmc = "842" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Мокко, оч.св.", CodeGamma = "3042", CodeDmc = "ECRU" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Коричнево-серый, оч.св.", CodeGamma = "3043", CodeDmc = "3072" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Коричнево-серый, св.", CodeGamma = "3044", CodeDmc = "648" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Оловянный оч.св.", CodeGamma = "3045", CodeDmc = "928,3024" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Оловянный", CodeGamma = "3046", CodeDmc = "169" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Сине-фиолетовый, оч.св.", CodeGamma = "3047", CodeDmc = "157" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Сине-фиолетовый", CodeGamma = "3048", CodeDmc = "156" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Серо-синий, св.", CodeGamma = "3049", CodeDmc = "3752" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Серо-синий", CodeGamma = "3050", CodeDmc = "160" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Нежно голубой, св.", CodeGamma = "3051", CodeDmc = "775" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Синий, св.", CodeGamma = "3052", CodeDmc = "322,334" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Голубой, св.", CodeGamma = "3053", CodeDmc = "928" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Небесно-голубой", CodeGamma = "3054", CodeDmc = "519" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Джинсовый, св.", CodeGamma = "3055", CodeDmc = "518" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Бирюзовый", CodeGamma = "3056", CodeDmc = "598" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Бирюзовый, т.", CodeGamma = "3057", CodeDmc = "597" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Бирюзовый св.", CodeGamma = "3058", CodeDmc = "3849" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Морская волна, св.", CodeGamma = "3059", CodeDmc = "3848" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Аквамарин, св.", CodeGamma = "3060", CodeDmc = "992" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Серо-зеленый, св.", CodeGamma = "3061", CodeDmc = "3817" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Серо-зеленый", CodeGamma = "3062", CodeDmc = "3816" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Серо-зеленый, т.", CodeGamma = "3063", CodeDmc = "163" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Серо-зеленый", CodeGamma = "3064", CodeDmc = "3053" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Серо-зеленый", CodeGamma = "3065", CodeDmc = "522,3022" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Хаки, св.", CodeGamma = "3066", CodeDmc = "372,3013" },

            //// === Page 9 ===
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Хаки", CodeGamma = "3067", CodeDmc = "3012,3364" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Горчичный, св.", CodeGamma = "3068", CodeDmc = "370,371" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Оливково-золотой, оч.св.", CodeGamma = "3069", CodeDmc = "834" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Оловянно-серый, оч.т.", CodeGamma = "3070", CodeDmc = "3799" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Лососевый", CodeGamma = "3071", CodeDmc = "3712" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Лососевый, т.", CodeGamma = "3072", CodeDmc = "3328" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Кораллово-красный, св.", CodeGamma = "3073", CodeDmc = "352" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Розовый", CodeGamma = "3074", CodeDmc = "3326" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Розовый, яркий", CodeGamma = "3075", CodeDmc = "760" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Красный, яркий, св.", CodeGamma = "3076", CodeDmc = "891" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Гвоздичный, оч.св.", CodeGamma = "3077", CodeDmc = "894" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Гвоздичный", CodeGamma = "3078", CodeDmc = "3712" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Герань, оч.св.", CodeGamma = "3079", CodeDmc = "604,957" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Герань", CodeGamma = "3080", CodeDmc = "956" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Пыльной розы, оч.св.", CodeGamma = "3081", CodeDmc = "3716" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Пыльной розы", CodeGamma = "3082", CodeDmc = "3354" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Пыльной розы, т.", CodeGamma = "3083", CodeDmc = "961" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Малиновый, св.", CodeGamma = "3084", CodeDmc = "3354,3733" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Малиновый", CodeGamma = "3085", CodeDmc = "3832" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Малиновый, т.", CodeGamma = "3086", CodeDmc = "309" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Розовый, св.", CodeGamma = "3087", CodeDmc = "761" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Розовый", CodeGamma = "3088", CodeDmc = "899" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Розовый, т.", CodeGamma = "3089", CodeDmc = "335" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Малиновый, оч.т.", CodeGamma = "3090", CodeDmc = "309,3831" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Пыльной розы, св.", CodeGamma = "3091", CodeDmc = "3354" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Пыльной розы, т.", CodeGamma = "3092", CodeDmc = "3731" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Розовато-лиловый, св.", CodeGamma = "3093", CodeDmc = "3689" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Розовато-лиловый, т.", CodeGamma = "3094", CodeDmc = "3803" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Розово-бежевый, св.", CodeGamma = "3095", CodeDmc = "152" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Розово-бежевый, т.", CodeGamma = "3096", CodeDmc = "3721" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Розово-бежевый, оч.т.", CodeGamma = "3097", CodeDmc = "221" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Античный розовато-лиловый", CodeGamma = "3098", CodeDmc = "316" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Античный розовато-лиловый, т.", CodeGamma = "3099", CodeDmc = "315" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Античный розовато-лиловый, оч.т.", CodeGamma = "3100", CodeDmc = "3802" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Антично-фиолетовый, т.", CodeGamma = "3101", CodeDmc = "3740" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Красного винограда, т.", CodeGamma = "3102", CodeDmc = "3834" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Розового цикламена, т.", CodeGamma = "3103", CodeDmc = "3804" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Клюквенный, т.", CodeGamma = "3104", CodeDmc = "601" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Клюквенный, оч.т.", CodeGamma = "3105", CodeDmc = "601" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Сливовый, т.", CodeGamma = "3106", CodeDmc = "915" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Лаванда, т.", CodeGamma = "3107", CodeDmc = "209,554" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Фиолетовый, оч.т.", CodeGamma = "3108", CodeDmc = "327" },

            //// === Page 10 ===
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "", CodeGamma = "159", CodeDmc = "3747" },

            //// === Page 13 ===
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Пепельный, оч.св.", CodeGamma = "3230", CodeDmc = "535" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Жемчужно-серый, оч.св.", CodeGamma = "3231", CodeDmc = "762,3756" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Оловянно-серый", CodeGamma = "3232", CodeDmc = "317" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Оловянно-серый, т.", CodeGamma = "3233", CodeDmc = "413" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Пыльная роза, оч.т.", CodeGamma = "3235", CodeDmc = "150" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Пыльная роза, оч.св.", CodeGamma = "3236", CodeDmc = "818,963" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Красного винограда оч.т.", CodeGamma = "3237", CodeDmc = "154" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Сине-фиолетовый", CodeGamma = "3238", CodeDmc = "155" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Васильковый, оч.св.", CodeGamma = "3239", CodeDmc = "157" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Васильковый оч.т.", CodeGamma = "3240", CodeDmc = "158" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Серо-синий, оч.св.", CodeGamma = "3241", CodeDmc = "3752" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Серо-синий", CodeGamma = "3242", CodeDmc = "161" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Голубой, оч.св.", CodeGamma = "3243", CodeDmc = "168" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Мох, оч.св.", CodeGamma = "3244", CodeDmc = "165" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Мох, св.", CodeGamma = "3245", CodeDmc = "3819" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Нефрит", CodeGamma = "3246", CodeDmc = "505" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Бежево-серый, ср.", CodeGamma = "3247", CodeDmc = "644" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Топаз", CodeGamma = "3248", CodeDmc = "3821" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Какао, т.", CodeGamma = "3249", CodeDmc = "08" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Снежно белый", CodeGamma = "3250", CodeDmc = "3865" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Фиолетовый, оч.св.", CodeGamma = "3251", CodeDmc = "153" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Зеленый лесной, св.", CodeGamma = "3252", CodeDmc = "164" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Желтовато-бежевый, оч.т.", CodeGamma = "3253", CodeDmc = "869" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Серо-синий, оч.т.", CodeGamma = "3254", CodeDmc = "312,803" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Серый, оч.оч.св.", CodeGamma = "3255", CodeDmc = "648" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Серый, оч.св.", CodeGamma = "3256", CodeDmc = "415" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Серый, св.", CodeGamma = "3257", CodeDmc = "-" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Серый", CodeGamma = "3258", CodeDmc = "317" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Серый, т.", CodeGamma = "3259", CodeDmc = "413" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Серый, оч.т.", CodeGamma = "3260", CodeDmc = "535" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Рождественский красный", CodeGamma = "4055", CodeDmc = "304,309" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Нежно зеленый", CodeGamma = "4105", CodeDmc = "966" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Серый, оч.т.", CodeGamma = "4195", CodeDmc = "-" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Лимонный, оч.св.", CodeGamma = "5130", CodeDmc = "10" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Синий, оч.т.", CodeGamma = "5135", CodeDmc = "825,3842" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Лимонный, св.", CodeGamma = "5140", CodeDmc = "445,3078" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Канареечный, яркий", CodeGamma = "5165", CodeDmc = "-" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Голубой", CodeGamma = "5166", CodeDmc = "-" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Голубой, т.", CodeGamma = "5167", CodeDmc = "996" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Синий", CodeGamma = "5168", CodeDmc = "995" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Серо-голубой", CodeGamma = "5169", CodeDmc = "519" },

            //// === Page 14 ===
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Сине-серый, т.", CodeGamma = "5171", CodeDmc = "312" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Синий, т.", CodeGamma = "5172", CodeDmc = "336" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Голубой, оч.св.", CodeGamma = "5173", CodeDmc = "3756" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Морская волна", CodeGamma = "5174", CodeDmc = "807" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Морская волна, оч.т.", CodeGamma = "5175", CodeDmc = "-" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Желтый", CodeGamma = "5176", CodeDmc = "745,3823" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Желтый, т.", CodeGamma = "5177", CodeDmc = "3820" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Оранжевый, т.", CodeGamma = "5178", CodeDmc = "-" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Желтый, оч.оч.св.", CodeGamma = "5179", CodeDmc = "3865" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Лимонный", CodeGamma = "5180", CodeDmc = "445" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Желтый, яркий", CodeGamma = "5181", CodeDmc = "444" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Серый, оч.св.", CodeGamma = "5182", CodeDmc = "762" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Серый, оч.оч.т.", CodeGamma = "5184", CodeDmc = "-" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Сине-серый, т.", CodeGamma = "5185", CodeDmc = "930" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Серый", CodeGamma = "5186", CodeDmc = "926" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Серый, т.", CodeGamma = "5188", CodeDmc = "926" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Серо-зеленый", CodeGamma = "5191", CodeDmc = "3787" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Серо-зеленый, т.", CodeGamma = "5192", CodeDmc = "934" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Бежево-сиреневый", CodeGamma = "5193", CodeDmc = "3042" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Серо-сиреневый", CodeGamma = "5194", CodeDmc = "28" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Зеленый, оч.т.", CodeGamma = "5195", CodeDmc = "500" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Салатовый, св.", CodeGamma = "5196", CodeDmc = "772" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Бирюзовый", CodeGamma = "5197", CodeDmc = "958" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Бирюзовый, т.", CodeGamma = "5198", CodeDmc = "3812" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Коричнево-красный, т.", CodeGamma = "5199", CodeDmc = "3857" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Бежевый, св.", CodeGamma = "5200", CodeDmc = "948" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Бежевый", CodeGamma = "5201", CodeDmc = "754" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Коричневый, св.", CodeGamma = "5202", CodeDmc = "3778" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Коричнево-красный", CodeGamma = "5203", CodeDmc = "3858" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Сиреневый, оч.т.", CodeGamma = "5204", CodeDmc = "550" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Фиолетовый, оч.т.", CodeGamma = "5205", CodeDmc = "154" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Сиреневый, св.", CodeGamma = "5206", CodeDmc = "25" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Мятный, св.", CodeGamma = "5207", CodeDmc = "955" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Мятный", CodeGamma = "5208", CodeDmc = "954" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Зеленый, св.", CodeGamma = "5209", CodeDmc = "913" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Зеленый", CodeGamma = "5210", CodeDmc = "912" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Зеленый", CodeGamma = "5211", CodeDmc = "911" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Зеленый, т.", CodeGamma = "5212", CodeDmc = "910" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Бежевый, оч.св.", CodeGamma = "5213", CodeDmc = "819" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Бежевый, св.", CodeGamma = "5214", CodeDmc = "948" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Бежевый", CodeGamma = "5215", CodeDmc = "945" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Оранжево-красный", CodeGamma = "5216", CodeDmc = "721,946" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Розовый оч.св.", CodeGamma = "5217", CodeDmc = "819" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Бежево-розовый", CodeGamma = "5219", CodeDmc = "758" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Серый т.", CodeGamma = "5220", CodeDmc = "317" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Серый", CodeGamma = "5221", CodeDmc = "318" },

            //// === Page 15 ===
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Бирюзовый, оч.св.", CodeGamma = "6035", CodeDmc = "" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Морской воды, оч.т.", CodeGamma = "6075", CodeDmc = "3812" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Сливовый, ультра св.", CodeGamma = "6080", CodeDmc = "153" },
            //new Floss { Red = 0, Green = 0, Blue = 0, ColorHex = "#000000", ColorName = "Горчичный, оч.св.", CodeGamma = "6115", CodeDmc = "3348" },
        };

        /// <summary>
        /// Поиск ближайшего цвета по RGB
        /// </summary>
        public static Floss FindClosest(int red, int green, int blue)
        {
            if (Colors == null || Colors.Count == 0)
                return null;

            return Colors
                .OrderBy(f => GetColorDistance(red, green, blue, f.Red, f.Green, f.Blue))
                .FirstOrDefault();
        }

        private static double GetColorDistance(int r1, int g1, int b1, int r2, int g2, int b2)
        {
            return Math.Sqrt(Math.Pow(r1 - r2, 2) + Math.Pow(g1 - g2, 2) + Math.Pow(b1 - b2, 2));
        }

        /// <summary>
        /// Получить цвет по коду Gamma
        /// </summary>
        public static Floss GetByGammaCode(string code)
        {
            return Colors.FirstOrDefault(f => f.CodeGamma == code);
        }

        /// <summary>
        /// Получить цвет по коду DMC
        /// </summary>
        public static Floss GetByDmcCode(string code)
        {
            return Colors.FirstOrDefault(f => f.CodeDmc != null && f.CodeDmc.Contains(code));
        }

        /// <summary>
        /// Получить все цвета
        /// </summary>
        public static List<Floss> GetAll()
        {
            return Colors;
        }
    }
}