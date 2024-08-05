import json
import time
from locust import between, task
from locust_plugins.users.socketio import SocketIOUser


class WebSocketUser(SocketIOUser):

    wait_time = between(1,2)

    @task
    def stream(self):
        wshost = self.host.replace("http://","ws://").replace("https://","wss://")

        try:
            self.connect(f"{wshost}/ws")

            self.sleep_with_heartbeat(120)

        except Exception as e:

            self.env.events.request.fire(
                request_type="websocket",
                name="price",  
                response_time=0,
                response_length=0,
                response=None,
                context=None,
                exception=e,
            )
            
        finally:
            self.stop();


    def on_message(self, message : str):

        msg_time = json.loads(message)["t"]
        cur_time = time.time() * 1000;
        self.environment.events.request.fire(
            request_type="websocket",
            name="price",  
            response_time = cur_time - msg_time, 
            response_length=len(message),
            response=message,
            context=None,
            exception=None,
        )