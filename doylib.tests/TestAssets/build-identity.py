"""
Generate the identity.onnx fixture used by OnnxInferenceServiceIntegrationTests.

The model: Identity(input: float32[N]) -> output: float32[N]
A single Identity op, dynamic 1-D shape.

Output: identity.onnx.
"""
import onnx
from onnx import helper, TensorProto

input_tensor = helper.make_tensor_value_info("input", TensorProto.FLOAT, ["N"])
output_tensor = helper.make_tensor_value_info("output", TensorProto.FLOAT, ["N"])

identity_node = helper.make_node("Identity", inputs=["input"], outputs=["output"])

graph = helper.make_graph(
    nodes=[identity_node],
    name="IdentityGraph",
    inputs=[input_tensor],
    outputs=[output_tensor],
)

model = helper.make_model(graph, producer_name="doylib-tests")
model.opset_import[0].version = 13
onnx.checker.check_model(model)
onnx.save(model, "identity.onnx")
print("Wrote identity.onnx")
