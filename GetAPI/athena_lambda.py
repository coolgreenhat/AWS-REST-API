import time
import boto3
import json

# athena constant
DATABASE = 'emp_db'
TABLE = 'emp_table'

# S3 constant
S3_OUTPUT = 's3://employeedg/'
S3_BUCKET = 'employeedg'

# number of retries
RETRY_COUNT = 10

def lambda_handler(event, context):

    # get keyword
    date = event['name']

    # created query
    query = "SELECT emp_name FROM %s.%s WHERE emp_dob = '%s';" % (DATABASE, TABLE, date)

    # athena client
    client = boto3.client('athena')

    # Execution
    response = client.start_query_execution(
        QueryString=query,
        QueryExecutionContext={
            'Database': DATABASE
        },
        ResultConfiguration={
            'OutputLocation': S3_OUTPUT,
        }
    )

    # get query execution id
    query_execution_id = response['QueryExecutionId']
    print(query_execution_id)

    # get execution status
    for i in range(1, 1 + RETRY_COUNT):

        # get query execution
        query_status = client.get_query_execution(QueryExecutionId=query_execution_id)
        query_execution_status = query_status['QueryExecution']['Status']['State']

        if query_execution_status == 'SUCCEEDED':
            print("STATUS:" + query_execution_status)
            break

        if query_execution_status == 'FAILED':
            raise Exception("STATUS:" + query_execution_status)

        else:
            print("STATUS:" + query_execution_status)
            time.sleep(i)
    else:
        client.stop_query_execution(QueryExecutionId=query_execution_id)
        raise Exception('TIME OVER')

    # get query results
    result = client.get_query_results(QueryExecutionId=query_execution_id)
    output = json.dumps(result)
    return output

