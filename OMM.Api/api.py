from flask import Flask, jsonify

# Create an instance of the Flask class that is the WSGI application.
# The first argument is the name of the application module or package,
# typically __name__ when using a single module.
app = Flask(__name__)

# Flask route decorators map / and /hello to the hello function.
# To add other resources, create functions that generate the page contents
# and add decorators to define the appropriate resource locators for them.

@app.route('/', methods=['GET'])
@app.route('/info', methods=['GET'])
def get_info():
    return jsonify({'info' : 'No info'})

if __name__ == '__main__':
    app.run('localhost', 55555)