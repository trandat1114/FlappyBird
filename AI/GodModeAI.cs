using System;
using System.Collections.Generic;
using System.Linq;
using FlappyBird.Models;
using FlappyBird.Utils;

namespace FlappyBird.AI
{
    /// <summary>
    /// AI điều khiển chim trong God mode - FAIR PLAY với học tập đơn giản
    /// </summary>
    public static class GodModeAI
    {
        private static readonly List<SimplePattern> learnedPatterns = new();
        private static DateTime lastLearningUpdate = DateTime.MinValue;
        private static DateTime lastJumpTime = DateTime.MinValue;
        
        /// <summary>
        /// Điều khiển chim tự động trong God mode - CONSERVATIVE SAFE PLAY
        /// </summary>
        public static void AutoControlBird(GameState gameState)
        {
            // **FAIR AI**: Uses EXACT same jump strength as human player
            float jumpStrength = GameState.JumpStrength; // NO CHEATING
            
            // **SAFE ZONE STRATEGY**: Luôn cố gắng giữ chim ở vùng an toàn (lower-middle)
            int safeZoneTop = GameState.GameHeight / 2 - 2;    // Trên vùng an toàn
            int safeZoneBottom = GameState.GameHeight - 5;     // Dưới vùng an toàn  
            
            // **LEARNING INTEGRATION**: Sử dụng độ cao đã học
            int learnedOptimalHeight = GetLearnedOptimalHeight();
            int idealHeight = learnedOptimalHeight; // Sử dụng độ cao đã học thay vì cố định
            
            // **DEBUG INFO**: Hiển thị trạng thái AI và learning
            Console.SetCursorPosition(1, GameState.GameHeight + 1);
            Console.Write($"[AI] Y:{gameState.BirdY:F1} V:{gameState.BirdVelocity:F2} Target:{idealHeight} Patterns:{learnedPatterns.Count}".PadRight(70));
            
            // **SIMPLE LEARNING**: Cập nhật kiến thức mỗi 5 giây
            if (DateTime.Now - lastLearningUpdate > TimeSpan.FromSeconds(5))
            {
                UpdateLearning();
                lastLearningUpdate = DateTime.Now;
            }
            
            // **ENHANCED CEILING PROTECTION** - Tránh va chạm ống trên cụ thể
            bool isNearCeiling = gameState.BirdY <= 8;
            bool isEmergencyFloor = gameState.BirdY >= GameState.GameHeight - 3; // Y >= 17
            
            // **PIPE-AWARE CEILING CHECK**: Kiểm tra xem có ống trên gần không
            bool willHitTopPipe = false;
            if (gameState.Pipes.Count > 0)
            {
                var nearPipe = gameState.Pipes.FirstOrDefault(p => p.X > GameState.BirdX - 10 && p.X < GameState.BirdX + 20);
                if (nearPipe != null)
                {
                    // Dự đoán vị trí sau jump
                    float predictedYAfterJump = gameState.BirdY + jumpStrength;
                    willHitTopPipe = predictedYAfterJump <= nearPipe.TopHeight + 1; // +1 buffer
                }
            }
            
            if ((isNearCeiling || willHitTopPipe) && !isEmergencyFloor)
            {
                Console.SetCursorPosition(1, GameState.GameHeight + 2);
                Console.Write($"AI: CEILING_PROTECTION - No Jump (Y:{gameState.BirdY:F1}, WillHit:{willHitTopPipe})".PadRight(60));
                return; // Chỉ thoát khi gần trần và không ở emergency floor
            }
            
            // **FIND NEAREST PIPE**: Tìm ống gần nhất - ENHANCED EARLY DETECTION
            Pipe? nearestPipe = null;
            foreach (var pipe in gameState.Pipes)
            {
                if (pipe.X > GameState.BirdX - 20) // Mở rộng detection range đáng kể
                {
                    nearestPipe = pipe;
                    break;
                }
            }
            
            bool shouldJump = false;
            string jumpReason = "";
            float optimalY = GameState.GameHeight / 2f; // Default value
            string targetSource = "Default";
            
            if (nearestPipe != null)
            {
                // **REVOLUTIONARY TARGET CALCULATION**: Sử dụng learned optimal position
                int gapTop = nearestPipe.TopHeight;
                int gapBottom = GameState.GameHeight - nearestPipe.BottomHeight;
                int distanceToPipe = nearestPipe.X - GameState.BirdX;
                
                // **LEARNED OPTIMAL POSITION**: Tìm pattern phù hợp
                var matchedPattern = learnedPatterns.FirstOrDefault(p => 
                    p.GapSize == nearestPipe.GapSize && 
                    Math.Abs(p.PipeTopHeight - nearestPipe.TopHeight) <= 1);
                
                int safetyMargin;
                
                if (matchedPattern != null && matchedPattern.Confidence > 0.3f)
                {
                    // **USE LEARNED OPTIMAL**: AI đã học được pattern này
                    optimalY = matchedPattern.OptimalY;
                    safetyMargin = matchedPattern.SafetyMargin;
                    targetSource = $"Learned(C:{matchedPattern.Confidence:F1})";
                }
                else
                {
                    // **THEORETICAL OPTIMAL**: PipeTop + GapSize/2
                    optimalY = nearestPipe.TopHeight + (nearestPipe.GapSize / 2f);
                    safetyMargin = nearestPipe.GapSize <= 7 ? 2 : 1;
                    targetSource = "Theoretical";
                }
                
                // **SAFE ZONES** dựa trên optimal position
                float pipeSafeTop = gapTop + safetyMargin;
                float pipeSafeBottom = gapBottom - safetyMargin;
                
                // **PREDICTION**: Dự đoán vị trí khi đến ống với gravity compensation
                float timeToReach = distanceToPipe / 8f;
                float gravityEffect = 0.5f * 0.08f * timeToReach * timeToReach; // s = 0.5*g*t² (gravity = 0.08f)
                float predictedY = gameState.BirdY + gameState.BirdVelocity * timeToReach + gravityEffect;
                
                // **SIMPLIFIED DECISION LOGIC**: Chỉ 3 rules rõ ràng, không xung đột
                
                // **CRITICAL CHECK**: Không nhảy nếu sẽ va chạm ống trên
                float jumpResultY = gameState.BirdY + jumpStrength;
                bool wouldHitTop = jumpResultY <= pipeSafeTop;
                
                // **RULE 1: EMERGENCY** - Tránh chạm đáy tuyệt đối (ưu tiên cao nhất)
                if (gameState.BirdY >= GameState.GameHeight - 3)
                {
                    shouldJump = true;
                    jumpReason = "EMERGENCY_FLOOR";
                }
                // **RULE 2: OPTIMAL TARGET** - Điều chỉnh về optimal position (core logic)
                else if (distanceToPipe <= 35 && gameState.BirdY > optimalY + 1.5f && !wouldHitTop)
                {
                    shouldJump = true;
                    jumpReason = $"TARGET_OPT_{targetSource}_{optimalY:F1}";
                }
                // **RULE 3: PREDICTION** - Dự đoán va chạm đáy dựa trên trajectory
                else if (distanceToPipe <= 25 && predictedY > pipeSafeBottom && !wouldHitTop)
                {
                    shouldJump = true;
                    jumpReason = "PREDICT_COLLISION";
                }
            }
            else
            {
                // **NO PIPE STRATEGY**: Giữ vùng an toàn khi không có ống
                
                // 1. **EMERGENCY FLOOR**: Tránh chạm đáy (ưu tiên cao nhất)
                if (gameState.BirdY >= GameState.GameHeight - 3)
                {
                    shouldJump = true;
                    jumpReason = "EMERGENCY_FLOOR";
                }
                // 1.5. **DANGEROUS BOTTOM**: Tránh vùng nguy hiểm gần đáy
                else if (gameState.BirdY >= GameState.GameHeight - 5 && gameState.BirdVelocity > 0.2f)
                {
                    shouldJump = true;
                    jumpReason = "DANGEROUS_BOTTOM";
                }
                // 2. **SAFE ZONE MAINTENANCE**: Về vùng an toàn (cân bằng)
                else if (gameState.BirdY > safeZoneBottom && gameState.BirdY > 10)
                {
                    shouldJump = true;
                    jumpReason = "BACK_TO_SAFE_ZONE";
                }
                // 3. **LEARNED OPTIMAL HEIGHT**: Điều chỉnh về độ cao đã học
                else if (Math.Abs(gameState.BirdY - idealHeight) > 3 && gameState.BirdY > idealHeight + 1 && gameState.BirdY > 11)
                {
                    shouldJump = true;
                    jumpReason = "LEARNED_HEIGHT_ADJUST";
                }
            }
            
            // **EXECUTE JUMP** với revolutionary debug log
            if (shouldJump)
            {
                gameState.BirdVelocity = jumpStrength;
                lastJumpTime = DateTime.Now;
                
                // **ENHANCED DEBUG** - hiển thị learned pattern information
                string debugInfo = nearestPipe != null ? 
                    $"Gap{nearestPipe.GapSize}Top{nearestPipe.TopHeight}->Opt{optimalY:F1}@Dist{nearestPipe.X - GameState.BirdX}" : 
                    "NoGap";
                
                Console.SetCursorPosition(1, GameState.GameHeight + 2);
                Console.Write($"AI: {jumpReason} {debugInfo} (Y:{gameState.BirdY:F1}→{gameState.BirdY + jumpStrength:F1})".PadRight(100));
            }
            else
            {
                string gapInfo = nearestPipe != null ? $"Gap:{nearestPipe.GapSize},Dist:{nearestPipe.X - GameState.BirdX}" : "NoGap";
                Console.SetCursorPosition(1, GameState.GameHeight + 2);
                Console.Write($"AI: COASTING {gapInfo} (Y:{gameState.BirdY:F1}, V:{gameState.BirdVelocity:F2})".PadRight(80));
            }
        }
        
        /// <summary>
        /// Aggressive AI strategy for comparison - jumps more frequently
        /// </summary>
        public static void AutoControlBirdAggressive(GameState gameState)
        {
            float jumpStrength = GameState.JumpStrength;
            
            // More aggressive safe zone - higher target
            int aggressiveTarget = GameState.GameHeight / 2 - 1; // Higher than conservative
            
            // Jump more frequently with less safety margin
            bool shouldJump = false;
            
            // Emergency situations
            bool isEmergencyFloor = gameState.BirdY >= GameState.GameHeight - 2;
            
            if (isEmergencyFloor)
            {
                shouldJump = true;
            }
            else
            {
                // Aggressive positioning - jump when below target
                if (gameState.BirdY > aggressiveTarget)
                {
                    shouldJump = true;
                }
                
                // Pipe awareness but with less margin
                var nearestPipe = gameState.Pipes.OrderBy(p => Math.Abs(p.X - GameState.BirdX)).FirstOrDefault();
                if (nearestPipe != null && nearestPipe.X - GameState.BirdX < 8)
                {
                    int pipeGapCenter = (nearestPipe.TopHeight + (GameState.GameHeight - nearestPipe.BottomHeight)) / 2;
                    if (gameState.BirdY > pipeGapCenter + 1) // Less safety margin
                    {
                        shouldJump = true;
                    }
                }
            }
            
            // Throttle jumps but less than conservative
            if (shouldJump && DateTime.Now - lastJumpTime > TimeSpan.FromMilliseconds(120)) // Faster than conservative
            {
                gameState.BirdVelocity = jumpStrength;
                lastJumpTime = DateTime.Now;
            }
        }
        
        /// <summary>
        /// Cập nhật học tập thông minh từ dữ liệu thất bại - FIXED ALGORITHM
        /// </summary>
        private static void UpdateLearning()
        {
            var failures = GameLogger.LoadGodModeFailures();
            if (failures.Count == 0) return;
            
            // **REVOLUTIONARY LEARNING**: Học theo (GapSize + PipeTopHeight) combo
            learnedPatterns.Clear();
            
            // **COMBO LEARNING**: Nhóm theo cả GapSize VÀ PipeTopHeight
            var comboGroups = failures.GroupBy(f => new { 
                f.PipeGapSize, 
                PipeTopGroup = (f.PipeTopHeight / 2) * 2  // Group theo 2: 4-5, 6-7, 8-9
            });
            
            foreach (var group in comboGroups)
            {
                int gapSize = group.Key.PipeGapSize;
                int topGroup = group.Key.PipeTopGroup;
                var groupFailures = group.ToList();
                
                if (groupFailures.Count >= 1) // Học từ ít nhất 1 lần thất bại
                {
                    var topCollisions = groupFailures.Count(f => f.CollisionType == "top");
                    var bottomCollisions = groupFailures.Count(f => f.CollisionType == "bottom");
                    
                    // **SIMPLIFIED LEARNING**: Chỉ học optimal position, không adjust phức tạp
                    float avgPipeTop = (float)groupFailures.Average(f => f.PipeTopHeight);
                    float theoreticalOptimal = avgPipeTop + (gapSize / 2f); // Center of gap
                    
                    // **SIMPLE ADJUSTMENT**: Chỉ adjust nhẹ dựa trên collision pattern
                    float learnedOptimal = theoreticalOptimal;
                    int safetyMargin = 1;
                    
                    if (topCollisions > bottomCollisions && topCollisions >= 2)
                    {
                        // Nhiều top collision → target hơi thấp hơn
                        learnedOptimal = theoreticalOptimal + 0.5f;
                        safetyMargin = 2;
                    }
                    else if (bottomCollisions > topCollisions && bottomCollisions >= 2)
                    {
                        // Nhiều bottom collision → target hơi cao hơn  
                        learnedOptimal = theoreticalOptimal - 0.5f;
                        safetyMargin = 2;
                    }
                    
                    // **STRICT SAFETY BOUNDS** - luôn trong gap
                    learnedOptimal = Math.Max(learnedOptimal, avgPipeTop + 2.0f);
                    learnedOptimal = Math.Min(learnedOptimal, avgPipeTop + gapSize - 2.0f);
                    
                    learnedPatterns.Add(new SimplePattern
                    {
                        GapSize = gapSize,
                        PipeTopHeight = (int)avgPipeTop,
                        OptimalY = learnedOptimal,
                        SafetyMargin = safetyMargin,
                        Confidence = Math.Min(groupFailures.Count / 2f, 1f),
                        TopCollisionRate = (float)topCollisions / groupFailures.Count,
                        BottomCollisionRate = (float)bottomCollisions / groupFailures.Count,
                        SampleSize = groupFailures.Count
                    });
                }
            }
            
            Console.SetCursorPosition(1, GameState.GameHeight + 3);
            Console.Write($"[LEARNING] Patterns: {learnedPatterns.Count}, Data: {failures.Count} failures".PadRight(70));
            
            // **VALIDATION DEBUG**: Hiển thị learned patterns cho specific combos
            if (learnedPatterns.Count > 0)
            {
                Console.SetCursorPosition(1, GameState.GameHeight + 4);
                var gap8Top5 = learnedPatterns.FirstOrDefault(p => p.GapSize == 8 && Math.Abs(p.PipeTopHeight - 5) <= 1);
                if (gap8Top5 != null)
                {
                    Console.Write($"[GAP8-TOP5] Learned: OptY={gap8Top5.OptimalY:F1}, Conf={gap8Top5.Confidence:F2}, Samples={gap8Top5.SampleSize}".PadRight(70));
                }
            }
        }
        
        /// <summary>
        /// Lấy safety margin đã học cho gap size cụ thể
        /// </summary>
        private static int GetLearnedSafetyMargin(int gapSize)
        {
            var pattern = learnedPatterns.FirstOrDefault(p => p.GapSize == gapSize);
            return pattern?.SafetyMargin ?? 1; // Default safety margin
        }
        
        /// <summary>
        /// Kiểm tra vị trí có nguy hiểm dựa trên kinh nghiệm đã học
        /// </summary>
        private static bool IsLearnedDangerousPosition(int birdY, Pipe pipe)
        {
            var failures = GameLogger.LoadGodModeFailures();
            if (failures.Count < 5) return false;
            
            // **SỬA LỖI**: Không coi vị trí gần trần (Y <= 3) là nguy hiểm
            if (birdY <= 3) return false;
            
            // Tìm các lần thất bại tương tự (cùng gap size và vị trí tương tự)
            int dangerousCount = failures.Count(f => 
                f.PipeGapSize == pipe.GapSize &&
                Math.Abs(f.BirdY - birdY) <= 2 &&
                f.CollisionType == "top");
            
            int totalSimilar = failures.Count(f => 
                f.PipeGapSize == pipe.GapSize &&
                Math.Abs(f.BirdY - birdY) <= 2);
            
            // Nếu > 40% lần ở vị trí này bị va chạm ống trên → nguy hiểm
            return totalSimilar >= 3 && (float)dangerousCount / totalSimilar > 0.4f;
        }
        
        /// <summary>
        /// Lấy độ cao tối ưu đã học - THỰC SỰ HỌC TỪ THẤT BẠI
        /// </summary>
        private static int GetLearnedOptimalHeight()
        {
            var failures = GameLogger.LoadGodModeFailures();
            
            // **CONSERVATIVE**: Luôn target ở giữa màn hình, tránh cả trần và đáy
            int defaultHeight = GameState.GameHeight / 2; // Chính giữa
            
            if (failures.Count < 3) return defaultHeight;
            
            // **SMART LEARNING**: Phân tích chi tiết các loại va chạm
            var topCollisions = failures.Where(f => f.CollisionType == "top").ToList();
            var bottomCollisions = failures.Where(f => f.CollisionType == "bottom").ToList();
            
            int targetHeight = defaultHeight;
            
            // Nếu va chạm ống trên nhiều hơn → bay thấp hơn
            if (topCollisions.Count > bottomCollisions.Count && topCollisions.Count >= 3)
            {
                int avgTopCollisionHeight = (int)topCollisions.Average(f => f.BirdY);
                targetHeight = Math.Max(avgTopCollisionHeight + 4, GameState.GameHeight / 2 + 2);
            }
            // Nếu va chạm ống dưới nhiều hơn → bay cao hơn một chút
            else if (bottomCollisions.Count > topCollisions.Count && bottomCollisions.Count >= 3)
            {
                int avgBottomCollisionHeight = (int)bottomCollisions.Average(f => f.BirdY);
                targetHeight = Math.Min(avgBottomCollisionHeight - 3, GameState.GameHeight / 2 - 1);
            }
            
            // **STRICT LIMITS**: Đảm bảo không bay quá cao hoặc quá thấp
            targetHeight = Math.Max(targetHeight, GameState.GameHeight / 3); // Không quá cao (tránh trần)
            targetHeight = Math.Min(targetHeight, GameState.GameHeight * 2 / 3); // Không quá thấp (tránh đáy)
            
            return targetHeight;
        }
    }
    
    /// <summary>
    /// Pattern thông minh AI học được - ENHANCED VERSION
    /// </summary>
    public class SimplePattern
    {
        public int GapSize { get; set; }
        public int PipeTopHeight { get; set; }  // NEW: Pipe top height
        public float OptimalY { get; set; }     // NEW: Learned optimal position
        public int SafetyMargin { get; set; }
        public float Confidence { get; set; }
        public float TopCollisionRate { get; set; }
        public float BottomCollisionRate { get; set; }
        public int SampleSize { get; set; }     // NEW: Number of failures for this pattern
    }
}
