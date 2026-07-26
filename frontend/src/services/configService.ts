/**
 * Configuration Service
 *
 * Handles all configuration-related Tauri backend calls.
 * Pure 1-to-1 wrapper - no error handling changes, exact same behavior as direct invoke calls.
 */

import { safeInvoke } from '@/lib/isTauri';
import { TranscriptModelProps } from '@/components/TranscriptSettings';
import { ModelConfig } from '@/components/ModelSettingsModal';
import { CustomOpenAIConfig } from '@/components/CustomOpenAISettings';

export type { ModelConfig };
export type { CustomOpenAIConfig };

export interface RecordingPreferences {
  preferred_mic_device: string | null;
  preferred_system_device: string | null;
}

/**
 * Configuration Service
 * Singleton service for managing app configuration
 */
export class ConfigService {
  /**
   * Get saved transcript model configuration
   * @returns Promise with { provider, model, apiKey }
   */
  async getTranscriptConfig(): Promise<TranscriptModelProps> {
    return safeInvoke<TranscriptModelProps>('api_get_transcript_config', undefined, {
      provider: 'parakeet',
      model: 'parakeet-tdt-0.6b-v3-int8',
    });
  }

  /**
   * Get saved summary model configuration
   * @returns Promise with { provider, model, whisperModel }
   */
  async getModelConfig(): Promise<ModelConfig> {
    return safeInvoke<ModelConfig>('api_get_model_config', undefined, {
      provider: 'ollama',
      model: 'llama3:8b',
      whisperModel: 'large-v3',
    });
  }

  /**
   * Get saved audio device preferences
   * @returns Promise with { preferred_mic_device, preferred_system_device }
   */
  async getRecordingPreferences(): Promise<RecordingPreferences> {
    return safeInvoke<RecordingPreferences>('get_recording_preferences', undefined, {
      preferred_mic_device: null,
      preferred_system_device: null,
    });
  }

  /**
   * Get custom OpenAI configuration
   * @returns Promise with CustomOpenAIConfig or null if not configured
   */
  async getCustomOpenAIConfig(): Promise<CustomOpenAIConfig | null> {
    return safeInvoke<CustomOpenAIConfig | null>('api_get_custom_openai_config', undefined, null);
  }

  /**
   * Save custom OpenAI configuration
   * @param config CustomOpenAIConfig object
   */
  async saveCustomOpenAIConfig(config: CustomOpenAIConfig): Promise<void> {
    return safeInvoke<void>('api_save_custom_openai_config', { config });
  }

  /**
   * Test custom OpenAI connection
   * @param config CustomOpenAIConfig object
   * @returns Promise resolving if successful, rejecting with error message if failed
   */
  async testCustomOpenAIConnection(config: CustomOpenAIConfig): Promise<string> {
    return safeInvoke<string>('api_test_custom_openai_connection', { config }, 'OK');
  }
}

export const configService = new ConfigService();
