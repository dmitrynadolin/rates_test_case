from locust import FastHttpUser, task


class RestUser(FastHttpUser):
    wait_time = 1

    @task
    def rates(self):
        self.client.get("/api/rates")
    @task
    def pair(self):
        self.client.get("/api/rates/btcusd")