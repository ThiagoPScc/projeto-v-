using System;
using Unity.Netcode;
using UnityEngine;

public class NetcodeProximityChat : NetworkBehaviour
{
	private AudioClip micClip;
	private AudioSource audioSource;
	private const int sampleRate = 44100;
	private const int sampleLength = 1024; // Tamanho do buffer
	private bool isTransmitting = false;
	private float maxHearingDistance = 10f;

	void Start()
	{
		audioSource = GetComponent<AudioSource>();
		audioSource.loop = true;
		audioSource.Play();
	}

	public override void OnNetworkSpawn()
	{
		if (IsOwner)
		{
			StartMicrophone();
		}
	}

	void StartMicrophone()
	{
		micClip = Microphone.Start(null, true, 1, sampleRate);
		isTransmitting = true;
		InvokeRepeating(nameof(TransmitAudio), 0.1f, 0.1f); // Enviar pacotes a cada 0.1s
	}

	void TransmitAudio()
	{
		if (!isTransmitting || !IsOwner) return;

		float[] samples = new float[sampleLength];
		micClip.GetData(samples, Microphone.GetPosition(null) - sampleLength);

		byte[] audioData = ConvertFloatArrayToByteArray(samples);
		SendAudioServerRpc(audioData, NetworkManager.Singleton.LocalClientId);
	}

	[ServerRpc]
	void SendAudioServerRpc(byte[] audioData, ulong senderId)
	{
		BroadcastAudioClientRpc(audioData, senderId);
	}

	[ClientRpc]
	void BroadcastAudioClientRpc(byte[] audioData, ulong senderId)
	{
		if (senderId == NetworkManager.Singleton.LocalClientId) return;

		//float distance = Vector3.Distance(transform.position, NetworkManager.Singleton.ConnectedClients[senderId].PlayerObject.transform.position);
		//float volume = Mathf.Clamp01(1 - (distance / maxHearingDistance));

		PlayReceivedAudio(audioData, 1);
	}

	void PlayReceivedAudio(byte[] audioData, float volume)
	{
		float[] samples = ConvertByteArrayToFloatArray(audioData);
		AudioClip receivedClip = AudioClip.Create("ReceivedAudio", samples.Length, 1, sampleRate, false);
		receivedClip.SetData(samples, 0);

		audioSource.volume = volume;
		audioSource.clip = receivedClip;
		audioSource.Play();
	}

	// Conversão Float[] → Byte[]
	byte[] ConvertFloatArrayToByteArray(float[] floatArray)
	{
		short[] shortArray = new short[floatArray.Length];
		for (int i = 0; i < floatArray.Length; i++)
		{
			shortArray[i] = (short)(Mathf.Clamp(floatArray[i], -1.0f, 1.0f) * short.MaxValue);
		}

		byte[] byteArray = new byte[shortArray.Length * 2];
		Buffer.BlockCopy(shortArray, 0, byteArray, 0, byteArray.Length);
		return byteArray;
	}

	// Conversão Byte[] → Float[]
	float[] ConvertByteArrayToFloatArray(byte[] byteArray)
	{
		short[] shortArray = new short[byteArray.Length / 2];
		Buffer.BlockCopy(byteArray, 0, shortArray, 0, byteArray.Length);

		float[] floatArray = new float[shortArray.Length];
		for (int i = 0; i < shortArray.Length; i++)
		{
			floatArray[i] = shortArray[i] / (float)short.MaxValue;
		}
		return floatArray;
	}
}
