FROM python:3.12-alpine
WORKDIR /app
COPY app.py telemetry.py ./
EXPOSE 8003
ENV PORT=8003
HEALTHCHECK --interval=15s --timeout=3s --retries=3 CMD wget -qO- http://localhost:8003/health || exit 1
CMD ["python3", "app.py"]
