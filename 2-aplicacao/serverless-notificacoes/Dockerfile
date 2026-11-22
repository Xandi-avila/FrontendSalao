FROM python:3.12-alpine
WORKDIR /app
COPY lambda_handler.py telemetry.py ./
EXPOSE 8004
ENV PORT=8004
HEALTHCHECK --interval=15s --timeout=3s --retries=3 CMD wget -qO- http://localhost:8004/health || exit 1
CMD ["python3", "lambda_handler.py"]
