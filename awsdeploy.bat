echo cdk synth
call cdk synth

echo cdk bootstrap aws:///us-east-2
call cdk bootstrap aws:///us-east-2

echo cdk deploy
call cdk deploy