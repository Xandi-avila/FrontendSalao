FROM python:3.12-alpine
WORKDIR /app
COPY app.py telemetry.py ./
EXPOSE 8001
ENV PORT=8001
HEALTHCHECK --interval=15s --timeout=3s --retries=3 CMD wget -qO- http://127.0.0.1:8001/health || exit 1
CMD ["python3", "app.py"]
