
import '../datas/product_data.dart';

final List<ProductData> products = [
  ProductData(
    category: 'Baquetas',
    id: '1',
    title: 'Baquetas ',
    description: 'Baquetas de diversos tamanhos.',
    price: 39.90,
    imagens: ['assets/imagens/baquetacat.jpg'],
    sizes: ['2B', '5B', '5A', '7A'],
  ),
  ProductData(
    category: 'Baterias',
    id: '2',
    title: 'Bateria Acústica',
    description: 'Bateria para ensaios e apresentações.',
    price: 1899.90,
    imagens: ['assets/imagens/bateria.png'],
    sizes: ['Adulto'],
  ),
  ProductData(
    category: 'Guitarras',
    id: '3',
    title: 'Guitarra Elétrica',
    description: 'Guitarra para ensaios e apresentações.',
    price: 999.90,
    imagens: ['assets/imagens/guitarra.png'],
    sizes: ['Padrão'],
  ),
  ProductData(
    category: 'Baixos',
    id: '4',
    title: 'Baixo Elétrico',
    description: 'Baixo para ensaios e apresentações.',
    price: 1199.90,
    imagens: ['assets/imagens/baixo.png'],
    sizes: ['4 cordas', '5 cordas'],
  ),
];
