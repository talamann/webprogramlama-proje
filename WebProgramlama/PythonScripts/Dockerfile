FROM python:3.9-slim

WORKDIR /app

RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt

COPY flask_app.py .

EXPOSE 5000

ENV DOCKER_ENV=true

CMD ["python", "flask_app.py"]