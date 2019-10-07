#include <iostream>
#include <fstream>
#include <string>
#include <vector>

#include "yariv.h"

static std::vector<char> ReadBinaryFile(const char* fileName)
{
	std::ifstream ifs(fileName, std::ios::binary | std::ios::ate);
	std::ifstream::pos_type pos = ifs.tellg();

	std::vector<char> result(pos);

	ifs.seekg(0, std::ios::beg);
	ifs.read(&result[0], pos);

	return result;
}

enum InputLanguage
{
	Spirv,
	Yariv
};

int main(int argc, const char* argv[])
{
	auto shaderBytes = ReadBinaryFile(argv[1]);

	auto inputLanguage = (InputLanguage)std::stoi(argv[2]);
	auto encodeFlags = (yariv_encode_flags_e)std::stoi(argv[3]);

	int result;
	std::vector<char> outputBytes;

	switch (inputLanguage)
	{
	case Spirv:
		outputBytes.resize(yariv_encode_size(encodeFlags, shaderBytes.data(), shaderBytes.size()));
		result = yariv_encode(encodeFlags, outputBytes.data(), outputBytes.size(), shaderBytes.data(), shaderBytes.size());
		break;

	case Yariv:
		outputBytes.resize(yariv_decode_size(shaderBytes.data(), shaderBytes.size()));
		result = yariv_decode(outputBytes.data(), outputBytes.size(), shaderBytes.data(), shaderBytes.size());
		break;

	default:
		return 1;
	}

	std::ofstream out(argv[4], std::ios::binary | std::ios::out);
	out.write((char*)outputBytes.data(), outputBytes.size());
	out.close();

	return 0;
}

