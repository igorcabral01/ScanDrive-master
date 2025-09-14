# Copia o entrypoint
COPY entrypoint.sh .

# Dá permissão de execução
RUN chmod +x ./entrypoint.sh

# Usa o script como entrypoint
ENTRYPOINT ["./entrypoint.sh"]
