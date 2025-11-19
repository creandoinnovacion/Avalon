using TransportQuote.Models;

namespace TransportQuote.Services;

public class LocationService
{
    public IEnumerable<Location> GetLocations()
    {
        return new List<Location>
        {
            new Location("1", "Playa Delfines", "Playa", 21.0633, -86.7805, "Playa icónica famosa por su agua azul intenso y el letrero de Cancún.", "https://upload.wikimedia.org/wikipedia/commons/thumb/c/c2/Playa_Delfines_Cancun.jpg/800px-Playa_Delfines_Cancun.jpg"),
            new Location("2", "Parque Xcaret", "Atracción", 20.5809, -87.1197, "Parque ecoarqueológico con ríos subterráneos y espectáculos culturales.", "https://upload.wikimedia.org/wikipedia/commons/thumb/4/46/Xcaret_Park.jpg/800px-Xcaret_Park.jpg"),
            new Location("3", "Ruinas de Tulum", "Atracción", 20.2148, -87.4292, "Antiguo puerto maya sobre un acantilado con vista al Caribe.", "https://upload.wikimedia.org/wikipedia/commons/thumb/3/32/Tulum_Ruins.jpg/800px-Tulum_Ruins.jpg"),
            new Location("4", "Hotel Xcaret México", "Hotel", 20.5855, -87.1123, "Resort de lujo integrado con la selva, los ríos y la cultura local.", "https://cf.bstatic.com/xdata/images/hotel/max1024x768/167939086.jpg?k=123"),
            new Location("5", "Playa Norte (Isla Mujeres)", "Playa", 21.2636, -86.7508, "Reconocida por su arena blanca y aguas calmadas y cristalinas.", "https://upload.wikimedia.org/wikipedia/commons/thumb/f/f3/Playa_Norte_Isla_Mujeres.jpg/800px-Playa_Norte_Isla_Mujeres.jpg"),
            new Location("6", "Chichén Itzá", "Atracción", 20.6843, -88.5678, "Zona arqueológica maya Patrimonio de la Humanidad y Maravilla del Mundo.", "https://upload.wikimedia.org/wikipedia/commons/thumb/5/51/Chichen_Itza_3.jpg/800px-Chichen_Itza_3.jpg"),
            new Location("7", "Playa Tortugas", "Playa", 21.1421, -86.7431, "Playa pública muy concurrida con oleaje suave y muelle hacia Isla Mujeres.", "https://example.com/locations/playa-tortugas.jpg"),
            new Location("8", "Playa Fórum", "Playa", 21.1337, -86.7483, "Playa animada detrás de la zona de antros con arena suave y ambiente nocturno.", "https://example.com/locations/playa-forum.jpg"),
            new Location("9", "Playa Marlín", "Playa", 21.0879, -86.7784, "Amplia playa pública de aguas turquesa junto a centros comerciales.", "https://example.com/locations/playa-marlin.jpg"),
            new Location("10", "Playa Ballenas", "Playa", 21.0895, -86.7702, "Extenso arenal ideal para caminar y practicar bodyboard.", "https://example.com/locations/playa-ballenas.jpg"),
            new Location("11", "Playa Chac Mool", "Playa", 21.133, -86.7447, "Playa céntrica con clubes de playa, voleibol y deportes acuáticos.", "https://example.com/locations/playa-chac-mool.jpg"),
            new Location("12", "Playa Langosta", "Playa", 21.1512, -86.7821, "Espacio familiar con agua baja, juegos infantiles y sombra.", "https://example.com/locations/playa-langosta.jpg"),
            new Location("13", "Playa Caracol", "Playa", 21.1371, -86.7417, "Bahía resguardada de olas suaves en el extremo norte de la zona hotelera.", "https://example.com/locations/playa-caracol.jpg"),
            new Location("14", "Playa Gaviota Azul", "Playa", 21.1312, -86.7471, "Famosa en redes por sus aguas azules junto al centro comercial Fórum.", "https://example.com/locations/playa-gaviota-azul.jpg"),
            new Location("15", "Playa Las Perlas", "Playa", 21.1548, -86.8092, "Pequeña playa local perfecta para paddle board en oleaje tranquilo.", "https://example.com/locations/playa-las-perlas.jpg"),
            new Location("16", "Punta Nizuc", "Playa", 21.0102, -86.8087, "Sitio silencioso para esnorquelear en el extremo sur de la zona hotelera.", "https://example.com/locations/punta-nizuc.jpg"),
            new Location("17", "Playa Coral (Mirador II)", "Playa", 21.0606, -86.7706, "Playa pet-friendly con mirador espectacular sobre el Caribe.", "https://example.com/locations/playa-coral.jpg"),
            new Location("18", "Isla Contoy", "Atracción", 21.4775, -86.7971, "Isla protegida ideal para avistar aves y disfrutar playas vírgenes.", "https://example.com/locations/isla-contoy.jpg"),
            new Location("19", "Parque Nacional Arrecife de Puerto Morelos", "Atracción", 20.8423, -86.8727, "Área marina para esnórquel entre arrecifes coloridos al sur de Cancún.", "https://example.com/locations/puerto-morelos-reef.jpg"),
            new Location("20", "Laguna Nichupté", "Atracción", 21.1176, -86.7925, "Sistema de lagunas perfecto para kayak, lanchas rápidas y atardeceres.", "https://example.com/locations/laguna-nichupte.jpg"),
            new Location("21", "Museo Maya de Cancún", "Atracción", 21.0831, -86.7728, "Museo moderno con piezas mayas y senderos rodeados de selva.", "https://example.com/locations/museo-maya-cancun.jpg"),
            new Location("22", "Mercado 28", "Atracción", 21.1618, -86.8264, "Mercado tradicional repleto de artesanías, antojitos y souvenirs.", "https://example.com/locations/mercado-28.jpg"),
            new Location("23", "Zona Arqueológica El Rey", "Atracción", 21.0805, -86.7747, "Pequeño sitio maya en la zona hotelera habitado por iguanas amigables.", "https://example.com/locations/el-rey-ruins.jpg"),
            new Location("24", "Ventura Park", "Atracción", 21.0399, -86.8424, "Parque familiar con toboganes, go-karts y experiencias con delfines.", "https://example.com/locations/ventura-park.jpg"),
            new Location("25", "Acuario Interactivo Cancún", "Atracción", 21.1257, -86.7482, "Acuario táctil donde se alimentan rayas, tiburones y lobos marinos.", "https://example.com/locations/interactive-aquarium.jpg"),
            new Location("26", "Plaza La Isla", "Atracción", 21.1244, -86.7484, "Centro comercial sobre canales con boutiques, gastronomía y entretenimiento.", "https://example.com/locations/la-isla-shopping.jpg"),
            new Location("27", "Coco Bongo Cancún", "Atracción", 21.1325, -86.7456, "Antro icónico con espectáculos acrobáticos y fiestas temáticas.", "https://example.com/locations/coco-bongo.jpg"),
            new Location("28", "Hotel Riu Palace Peninsula", "Hotel", 21.1452, -86.7847, "Resort todo incluido con vistas panorámicas a la laguna y al mar.", "https://example.com/locations/riu-palace-peninsula.jpg"),
            new Location("29", "Dreams Sands Cancún Resort & Spa", "Hotel", 21.1468, -86.8002, "Hotel familiar frente a playa de aguas calmadas con actividades infantiles.", "https://example.com/locations/dreams-sands.jpg"),
            new Location("30", "Grand Fiesta Americana Coral Beach Cancún", "Hotel", 21.1393, -86.7413, "Suites de lujo con spa galardonado y gastronomía reconocida.", "https://example.com/locations/coral-beach.jpg"),
            new Location("31", "Hyatt Ziva Cancún", "Hotel", 21.1398, -86.7419, "Resort todo incluido sobre una península con experiencias con delfines.", "https://example.com/locations/hyatt-ziva-cancun.jpg"),
            new Location("32", "JW Marriott Cancún Resort & Spa", "Hotel", 21.0819, -86.7716, "Hotel elegante frente al mar con restaurantes gourmet e infinity pools.", "https://example.com/locations/jw-marriott-cancun.jpg"),
            new Location("33", "The Ritz-Carlton, Cancún", "Hotel", 21.0846, -86.7726, "Propiedad icónica con servicio personalizado, cabañas y clases de cocina.", "https://example.com/locations/ritz-carlton-cancun.jpg"),
            new Location("34", "Secrets The Vine Cancún", "Hotel", 21.0791, -86.7701, "Resort sólo para adultos centrado en el vino y la alta cocina.", "https://example.com/locations/secrets-the-vine.jpg"),
            new Location("35", "Nizuc Resort & Spa", "Hotel", 21.0237, -86.8455, "Retiro de lujo con villas privadas y vistas a manglares y mar.", "https://example.com/locations/nizuc-resort.jpg"),
            new Location("36", "Hard Rock Hotel Cancún", "Hotel", 21.0875, -86.7737, "Resort temático musical con entretenimiento en vivo.", "https://example.com/locations/hard-rock-cancun.jpg"),
            new Location("37", "Moon Palace Cancún", "Hotel", 20.9646, -86.8548, "Complejo masivo con campo de golf, FlowRider y conciertos.", "https://example.com/locations/moon-palace.jpg"),
            new Location("38", "Iberostar Selection Cancún", "Hotel", 21.072, -86.7776, "Resort con campo de golf y arquitectura inspirada en pirámides.", "https://example.com/locations/iberostar-selection.jpg"),
            new Location("39", "Live Aqua Beach Resort Cancún", "Hotel", 21.125, -86.7512, "Oasis sensorial sólo para adultos con gastronomía gourmet y aromaterapia.", "https://example.com/locations/live-aqua.jpg"),
            new Location("40", "Paradisus Cancún", "Hotel", 21.0674, -86.7766, "Complejo de cuatro pirámides con atrios verdes y spa sereno.", "https://example.com/locations/paradisus-cancun.jpg"),
            new Location("41", "Royalton Riviera Cancún", "Hotel", 21.0109, -86.8602, "Resort moderno todo incluido con parque acuático y Diamond Club.", "https://example.com/locations/royalton-riviera.jpg"),
            new Location("42", "Excellence Playa Mujeres", "Hotel", 21.2464, -86.8058, "Escapada romántica sólo adultos con suites que incluyen albercas privadas.", "https://example.com/locations/excellence-playa-mujeres.jpg"),
            new Location("43", "TRS Coral Hotel", "Hotel", 21.2473, -86.8067, "Hotel premium sólo adultos dentro de Costa Mujeres con diseño contemporáneo.", "https://example.com/locations/trs-coral.jpg"),
            new Location("44", "Grand Palladium Costa Mujeres Resort & Spa", "Hotel", 21.2452, -86.8093, "Resort amplio con embarcaciones por canales, spa Zentropia y áreas familiares.", "https://example.com/locations/grand-palladium-costa-mujeres.jpg"),
            new Location("45", "Atelier Playa Mujeres", "Hotel", 21.2458, -86.8122, "Todo incluido inspirado en el arte con golf y swim-up suites.", "https://example.com/locations/atelier-playa-mujeres.jpg"),
            new Location("46", "Breathless Riviera Cancún Resort & Spa", "Hotel", 20.8886, -86.8684, "Resort vibrante sólo adultos con pool parties y noches temáticas.", "https://example.com/locations/breathless-riviera-cancun.jpg"),
            new Location("47", "Dreams Playa Mujeres Golf & Spa Resort", "Hotel", 21.2446, -86.8111, "Complejo familiar con río lento y hábitat de delfines.", "https://example.com/locations/dreams-playa-mujeres.jpg"),
            new Location("48", "Mirador Cancún", "Atracción", 21.0635, -86.7796, "Mirador emblemático con letras coloridas y vista panorámica al mar.", "https://example.com/locations/mirador-cancun.jpg"),
            new Location("49", "Parque Urbano Kabah", "Atracción", 21.1438, -86.8384, "Reserva urbana ideal para trotar entre cenotes y fauna local.", "https://example.com/locations/parque-kabah.jpg"),
            new Location("50", "Planetario Ka Yok", "Atracción", 21.1457, -86.8298, "Planetario con funciones de astronomía y exhibiciones de cosmovisión maya.", "https://example.com/locations/planetario-ka-yok.jpg"),
            new Location("51", "Xoximilco Cancún", "Atracción", 21.0904, -86.8422, "Fiesta flotante en trajineras con música mariachi y gastronomía mexicana.", "https://example.com/locations/xoximilco-cancun.jpg"),
            new Location("52", "Aquaworld Cancún", "Atracción", 21.0869, -86.7769, "Marina de servicios completos para buceo, parasailing y tours por la jungla.", "https://example.com/locations/aquaworld-cancun.jpg"),
            new Location("53", "Cena Show Pirata Captain Hook", "Atracción", 21.1506, -86.8011, "Experiencia familiar a bordo de galeones con cena y combates piratas.", "https://example.com/locations/captain-hook-cancun.jpg"),
            new Location("54", "Tortugranja (Isla Mujeres)", "Atracción", 21.263, -86.7334, "Centro de conservación de tortugas marinas con recorridos educativos.", "https://example.com/locations/tortugranja.jpg"),
            new Location("55", "Parque Natural Garrafón", "Atracción", 21.223, -86.7119, "Eco-parque en Isla Mujeres con esnórquel, tirolesas y hamacas.", "https://example.com/locations/garrafon-park.jpg"),
            new Location("56", "Río Secreto", "Atracción", 20.5643, -87.1143, "Río subterráneo con cavernas cristalinas y recorridos guiados.", "https://example.com/locations/rio-secreto.jpg"),
            new Location("57", "Aeropuerto Internacional de Cancún (CUN)", "Aeropuerto", 21.0415, -86.8743, "Principal aeropuerto de la región con cuatro terminales conectadas.", "https://upload.wikimedia.org/wikipedia/commons/6/6a/Cancun_Airport_terminal_3.jpg"),
            new Location("58", "Aeropuerto Internacional de Cozumel (CZM)", "Aeropuerto", 20.5216, -86.9256, "Terminal que conecta la isla con principales destinos nacionales e internacionales.", "https://upload.wikimedia.org/wikipedia/commons/f/f0/Cozumel_Airport.jpg"),
            new Location("59", "Aeródromo de Isla Mujeres", "Aeropuerto", 21.2611, -86.7504, "Pequeño aeródromo para vuelos privados y traslados rápidos desde Cancún.", "https://upload.wikimedia.org/wikipedia/commons/f/f0/Isla_Mujeres_Airport.jpg")
        };
    }
}
