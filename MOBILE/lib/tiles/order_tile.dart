import 'package:flutter/material.dart';

class OrderTile extends StatefulWidget {
  final String orderId;
  final DateTime orderDate;
  final int itemCount;
  final double total;
  final double? subtotal;
  final double? valorFrete;
  final double? valorDesconto;
  final String? codigoCupom;
  final List<dynamic> itens;
  final int currentStep;

  const OrderTile(
    this.orderId, {
    required this.orderDate,
    required this.itemCount,
    required this.total,
    this.subtotal,
    this.valorFrete,
    this.valorDesconto,
    this.codigoCupom,
    this.itens = const [],
    this.currentStep = 1,
    super.key,
  });

  @override
  State<OrderTile> createState() => _OrderTileState();
}

class _OrderTileState extends State<OrderTile> {
  bool _isExpanded = false;

  Widget _buildStep(int number, String label) {
    final bool completed = widget.currentStep >= number;

    return Expanded(
      child: Column(
        children: [
          CircleAvatar(
            radius: 16,
            backgroundColor: completed ? Colors.green : Colors.grey.shade300,
            child: Text(
              '$number',
              style: TextStyle(
                color: completed ? Colors.white : Colors.grey.shade700,
                fontWeight: FontWeight.bold,
                fontSize: 12,
              ),
            ),
          ),
          const SizedBox(height: 6),
          Text(
            label,
            textAlign: TextAlign.center,
            style: TextStyle(
              fontSize: 11,
              color: completed ? Colors.green.shade800 : Colors.grey.shade600,
              fontWeight: completed ? FontWeight.bold : FontWeight.normal,
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildProgressLine(bool completed) {
    return Expanded(
      child: Container(
        height: 3,
        margin: const EdgeInsets.only(bottom: 20),
        color: completed ? Colors.green : Colors.grey.shade300,
      ),
    );
  }

  void _showDetailsModal(BuildContext context) {
    showModalBottomSheet(
      context: context,
      isScrollControlled: true,
      shape: const RoundedRectangleBorder(
        borderRadius: BorderRadius.vertical(top: Radius.circular(20)),
      ),
      builder: (ctx) {
        final currency = 'R\$ ${widget.total.toStringAsFixed(2).replaceAll('.', ',')}';
        final dateFormat = MaterialLocalizations.of(ctx);

        return DraggableScrollableSheet(
          expand: false,
          initialChildSize: 0.65,
          minChildSize: 0.4,
          maxChildSize: 0.9,
          builder: (context, scrollController) {
            return Padding(
              padding: const EdgeInsets.all(20.0),
              child: ListView(
                controller: scrollController,
                children: [
                  Center(
                    child: Container(
                      width: 40,
                      height: 4,
                      decoration: BoxDecoration(
                        color: Colors.grey.shade300,
                        borderRadius: BorderRadius.circular(2),
                      ),
                    ),
                  ),
                  const SizedBox(height: 16),
                  Row(
                    mainAxisAlignment: MainAxisAlignment.spaceBetween,
                    children: [
                      Text(
                        'Pedido #${widget.orderId}',
                        style: const TextStyle(
                          fontSize: 18,
                          fontWeight: FontWeight.bold,
                        ),
                      ),
                      Container(
                        padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
                        decoration: BoxDecoration(
                          color: Colors.green.shade50,
                          borderRadius: BorderRadius.circular(12),
                          border: Border.all(color: Colors.green.shade300),
                        ),
                        child: Text(
                          'Confirmado',
                          style: TextStyle(
                            color: Colors.green.shade800,
                            fontWeight: FontWeight.bold,
                            fontSize: 12,
                          ),
                        ),
                      ),
                    ],
                  ),
                  const SizedBox(height: 6),
                  Text(
                    'Data: ${dateFormat.formatMediumDate(widget.orderDate)}',
                    style: TextStyle(color: Colors.grey.shade600, fontSize: 13),
                  ),
                  const SizedBox(height: 16),
                  const Divider(),
                  const SizedBox(height: 12),
                  const Text(
                    'Itens Comprados',
                    style: TextStyle(
                      fontSize: 16,
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                  const SizedBox(height: 12),
                  if (widget.itens.isEmpty)
                    const Padding(
                      padding: EdgeInsets.symmetric(vertical: 16.0),
                      child: Text(
                        'Nenhum item discriminado para este pedido.',
                        style: TextStyle(color: Colors.grey),
                      ),
                    )
                  else
                    ...widget.itens.map((item) {
                      final nome = item['nomeProduto']?.toString() ?? 'Instrumento Musical';
                      final qtd = int.tryParse(item['quantidade']?.toString() ?? '1') ?? 1;
                      final unitario = double.tryParse(item['valorUnitario']?.toString() ?? '0') ?? 0.0;
                      final subtotalItem = double.tryParse(item['valorTotal']?.toString() ?? '0') ?? (unitario * qtd);

                      return Container(
                        margin: const EdgeInsets.only(bottom: 12),
                        padding: const EdgeInsets.all(12),
                        decoration: BoxDecoration(
                          color: Colors.grey.shade50,
                          borderRadius: BorderRadius.circular(8),
                          border: Border.all(color: Colors.grey.shade200),
                        ),
                        child: Row(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            Expanded(
                              child: Column(
                                crossAxisAlignment: CrossAxisAlignment.start,
                                children: [
                                  Text(
                                    nome,
                                    style: const TextStyle(
                                      fontWeight: FontWeight.bold,
                                      fontSize: 14,
                                    ),
                                  ),
                                  const SizedBox(height: 4),
                                  Text(
                                    '$qtd un x R\$ ${unitario.toStringAsFixed(2).replaceAll('.', ',')}',
                                    style: TextStyle(
                                      color: Colors.grey.shade600,
                                      fontSize: 12,
                                    ),
                                  ),
                                ],
                              ),
                            ),
                            Text(
                              'R\$ ${subtotalItem.toStringAsFixed(2).replaceAll('.', ',')}',
                              style: const TextStyle(
                                fontWeight: FontWeight.bold,
                                fontSize: 14,
                              ),
                            ),
                          ],
                        ),
                      );
                    }),
                  const SizedBox(height: 12),
                  const Divider(),
                  const SizedBox(height: 12),
                  if (widget.subtotal != null && widget.subtotal! > 0)
                    _buildFinanceRow('Subtotal dos Itens', 'R\$ ${widget.subtotal!.toStringAsFixed(2).replaceAll('.', ',')}'),
                  if (widget.valorDesconto != null && widget.valorDesconto! > 0)
                    _buildFinanceRow(
                      'Desconto ${widget.codigoCupom != null ? '(${widget.codigoCupom})' : ''}',
                      '– R\$ ${widget.valorDesconto!.toStringAsFixed(2).replaceAll('.', ',')}',
                      color: Colors.green.shade700,
                    ),
                  if (widget.valorFrete != null)
                    _buildFinanceRow(
                      'Frete',
                      widget.valorFrete == 0.0 ? 'Gratis' : 'R\$ ${widget.valorFrete!.toStringAsFixed(2).replaceAll('.', ',')}',
                      color: widget.valorFrete == 0.0 ? Colors.blue.shade700 : null,
                    ),
                  const SizedBox(height: 8),
                  _buildFinanceRow(
                    'Total Pago',
                    currency,
                    isTotal: true,
                  ),
                  const SizedBox(height: 24),
                  SizedBox(
                    width: double.infinity,
                    child: ElevatedButton(
                      style: ElevatedButton.styleFrom(
                        backgroundColor: Theme.of(ctx).primaryColor,
                        foregroundColor: Colors.white,
                        padding: const EdgeInsets.symmetric(vertical: 12),
                      ),
                      onPressed: () => Navigator.of(ctx).pop(),
                      child: const Text('Fechar Detalhes'),
                    ),
                  ),
                ],
              ),
            );
          },
        );
      },
    );
  }

  Widget _buildFinanceRow(String label, String value, {Color? color, bool isTotal = false}) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 3.0),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.spaceBetween,
        children: [
          Text(
            label,
            style: TextStyle(
              fontSize: isTotal ? 16 : 13,
              fontWeight: isTotal ? FontWeight.bold : FontWeight.normal,
              color: isTotal ? Colors.black87 : Colors.grey.shade700,
            ),
          ),
          Text(
            value,
            style: TextStyle(
              fontSize: isTotal ? 16 : 13,
              fontWeight: isTotal ? FontWeight.bold : FontWeight.w600,
              color: color ?? (isTotal ? Colors.black87 : Colors.grey.shade900),
            ),
          ),
        ],
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    final dateFormat = MaterialLocalizations.of(context);
    final currency = 'R\$ ${widget.total.toStringAsFixed(2).replaceAll('.', ',')}';
    final int step = widget.currentStep.clamp(1, 3);

    return Card(
      margin: const EdgeInsets.symmetric(
        horizontal: 10,
        vertical: 8,
      ),
      elevation: 3,
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
      child: InkWell(
        borderRadius: BorderRadius.circular(12),
        onTap: () {
          setState(() {
            _isExpanded = !_isExpanded;
          });
        },
        child: Padding(
          padding: const EdgeInsets.all(16),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Row(
                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                children: [
                  Text(
                    'Pedido #${widget.orderId}',
                    style: const TextStyle(
                      fontSize: 16,
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                  Container(
                    padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 3),
                    decoration: BoxDecoration(
                      color: Colors.green.shade50,
                      borderRadius: BorderRadius.circular(8),
                      border: Border.all(color: Colors.green.shade200),
                    ),
                    child: Text(
                      'Confirmado',
                      style: TextStyle(
                        color: Colors.green.shade800,
                        fontWeight: FontWeight.bold,
                        fontSize: 11,
                      ),
                    ),
                  ),
                ],
              ),
              const SizedBox(height: 6),
              Text(
                'Data: ${dateFormat.formatMediumDate(widget.orderDate)}',
                style: TextStyle(color: Colors.grey.shade600, fontSize: 13),
              ),
              const SizedBox(height: 2),
              Text(
                '${widget.itemCount} produto(s) adquirido(s)',
                style: TextStyle(color: Colors.grey.shade700, fontSize: 13),
              ),
              const SizedBox(height: 6),
              Row(
                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                children: [
                  const Text(
                    'Valor Total Pago:',
                    style: TextStyle(
                      fontSize: 14,
                      color: Colors.grey,
                    ),
                  ),
                  Text(
                    currency,
                    style: TextStyle(
                      fontWeight: FontWeight.bold,
                      fontSize: 16,
                      color: Theme.of(context).primaryColor,
                    ),
                  ),
                ],
              ),
              const SizedBox(height: 16),
              const Divider(),
              const SizedBox(height: 12),
              Row(
                children: [
                  _buildStep(1, 'Preparacao'),
                  _buildProgressLine(step >= 2),
                  _buildStep(2, 'Transporte'),
                  _buildProgressLine(step >= 3),
                  _buildStep(3, 'Entrega'),
                ],
              ),
              const SizedBox(height: 12),
              Center(
                child: Text(
                  step == 1
                      ? 'Seu pedido esta sendo preparado.'
                      : step == 2
                          ? 'Seu pedido esta a caminho!'
                          : 'Seu pedido foi entregue!',
                  style: TextStyle(
                    color: Colors.green.shade700,
                    fontWeight: FontWeight.bold,
                    fontSize: 12,
                  ),
                ),
              ),
              const SizedBox(height: 12),
              const Divider(),

              // Botao de expansao / visualizacao de itens
              Row(
                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                children: [
                  TextButton.icon(
                    onPressed: () {
                      setState(() {
                        _isExpanded = !_isExpanded;
                      });
                    },
                    icon: Icon(
                      _isExpanded ? Icons.expand_less : Icons.expand_more,
                      size: 20,
                      color: Theme.of(context).primaryColor,
                    ),
                    label: Text(
                      _isExpanded ? 'Ocultar Produtos' : 'Ver Produtos (${widget.itens.length})',
                      style: TextStyle(
                        color: Theme.of(context).primaryColor,
                        fontWeight: FontWeight.bold,
                        fontSize: 13,
                      ),
                    ),
                  ),
                  TextButton.icon(
                    onPressed: () => _showDetailsModal(context),
                    icon: const Icon(Icons.open_in_new, size: 16),
                    label: const Text(
                      'Detalhes',
                      style: TextStyle(fontSize: 13),
                    ),
                  ),
                ],
              ),

              // Secao de produtos expandida inline
              if (_isExpanded) ...[
                const SizedBox(height: 8),
                Container(
                  padding: const EdgeInsets.all(12),
                  decoration: BoxDecoration(
                    color: Colors.grey.shade50,
                    borderRadius: BorderRadius.circular(8),
                    border: Border.all(color: Colors.grey.shade200),
                  ),
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      const Text(
                        'Produtos Comprados:',
                        style: TextStyle(
                          fontWeight: FontWeight.bold,
                          fontSize: 13,
                        ),
                      ),
                      const SizedBox(height: 8),
                      if (widget.itens.isEmpty)
                        const Text(
                          'Nenhum item detalhado disponivel.',
                          style: TextStyle(fontSize: 12, color: Colors.grey),
                        )
                      else
                        ...widget.itens.map((item) {
                          final nome = item['nomeProduto']?.toString() ?? 'Instrumento Musical';
                          final qtd = int.tryParse(item['quantidade']?.toString() ?? '1') ?? 1;
                          final unitario = double.tryParse(item['valorUnitario']?.toString() ?? '0') ?? 0.0;
                          final subtotalItem = double.tryParse(item['valorTotal']?.toString() ?? '0') ?? (unitario * qtd);

                          return Padding(
                            padding: const EdgeInsets.symmetric(vertical: 4.0),
                            child: Row(
                              children: [
                                Expanded(
                                  child: Text(
                                    nome,
                                    style: const TextStyle(
                                      fontSize: 13,
                                      fontWeight: FontWeight.w500,
                                    ),
                                  ),
                                ),
                                Text(
                                  '$qtd x R\$ ${unitario.toStringAsFixed(2).replaceAll('.', ',')}',
                                  style: TextStyle(
                                    fontSize: 12,
                                    color: Colors.grey.shade600,
                                  ),
                                ),
                                const SizedBox(width: 8),
                                Text(
                                  'R\$ ${subtotalItem.toStringAsFixed(2).replaceAll('.', ',')}',
                                  style: const TextStyle(
                                    fontSize: 13,
                                    fontWeight: FontWeight.bold,
                                  ),
                                ),
                              ],
                            ),
                          );
                        }),
                      if (widget.valorDesconto != null && widget.valorDesconto! > 0) ...[
                        const Divider(),
                        Row(
                          mainAxisAlignment: MainAxisAlignment.spaceBetween,
                          children: [
                            Text(
                              'Desconto ${widget.codigoCupom != null ? '(${widget.codigoCupom})' : ''}:',
                              style: TextStyle(fontSize: 12, color: Colors.green.shade700),
                            ),
                            Text(
                              '– R\$ ${widget.valorDesconto!.toStringAsFixed(2).replaceAll('.', ',')}',
                              style: TextStyle(
                                fontSize: 12,
                                fontWeight: FontWeight.bold,
                                color: Colors.green.shade700,
                              ),
                            ),
                          ],
                        ),
                      ],
                      if (widget.valorFrete != null) ...[
                        const SizedBox(height: 4),
                        Row(
                          mainAxisAlignment: MainAxisAlignment.spaceBetween,
                          children: [
                            const Text(
                              'Frete:',
                              style: TextStyle(fontSize: 12, color: Colors.grey),
                            ),
                            Text(
                              widget.valorFrete == 0.0
                                  ? 'Gratis'
                                  : 'R\$ ${widget.valorFrete!.toStringAsFixed(2).replaceAll('.', ',')}',
                              style: TextStyle(
                                fontSize: 12,
                                fontWeight: FontWeight.bold,
                                color: widget.valorFrete == 0.0 ? Colors.blue.shade700 : Colors.black87,
                              ),
                            ),
                          ],
                        ),
                      ],
                    ],
                  ),
                ),
              ],
            ],
          ),
        ),
      ),
    );
  }
}
